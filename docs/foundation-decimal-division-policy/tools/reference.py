"""Executable reference for the proposed exact-terminating subset; never a production runner."""
from math import gcd
from fractions import Fraction
from random import Random

class Refusal(Exception):
    """The selected finite policy cannot represent this operation."""


def _fits(n, digits):
    bits=abs(n).bit_length()
    if bits<=3*digits:return True
    if bits>4*digits:return False
    # Threshold is only needed near actual coefficient width, never solely from a huge caller limit.
    return abs(n)<10**digits


def _strip(n, factor, check):
    exponent=0
    while n and n%factor==0:
        check();n//=factor;exponent+=1
    return n,exponent


def divide(a,sa,b,sb,digits,scale,check=lambda:None):
    """Return exact (coefficient, scale); refuse nontermination/capacity without rounding."""
    check()
    if digits<=0 or not 0<=scale<=2147483647:raise ValueError('limits')
    for n,s in [(a,sa),(b,sb)]:
        if s<0 or s>2147483647:raise ValueError('scale')
        if s>scale or not _fits(n,digits):raise Refusal('input-capacity')
    if b==0:raise ZeroDivisionError('FOAR0001')
    if a==0:return 0,0
    common=gcd(abs(a),abs(b));check();n=abs(a)//common;d=abs(b)//common
    d,p=_strip(d,2,check);d,q=_strip(d,5,check)
    if d!=1:raise Refusal('nonterminating')
    n,u=_strip(n,2,check);n,v=_strip(n,5,check)
    k=max(p,q);s=k+sa-sb;e2=u+k-p;e5=v+k-q
    if s>0:
        cancel=min(e2,e5,s);e2-=cancel;e5-=cancel;s-=cancel
    elif s<0:e2-=s;e5-=s;s=0
    check()
    if s>scale or e2>=4*digits or e5>=2*digits:raise Refusal('result-capacity')
    for factor,exponent in [(2,e2),(5,e5)]:
        for _ in range(exponent):
            check()
            upper_bits=n.bit_length()+factor.bit_length()
            if upper_bits>3*digits:
                # Guard threshold has digits+1 places; refused result coefficient is never constructed.
                if upper_bits-1>4*digits or n>(10**digits-1)//factor:raise Refusal('result-capacity')
            n*=factor
    check()
    assert _fits(n,digits)
    return (-n if (a<0)!=(b<0) else n),s


def run_controls():
    """Compare the reference against an independent Fraction oracle on finite small-scale cases."""
    cases=[
        (1,0,8,0,8,8,(125,3)),(1,0,3,0,8,8,'nonterminating'),
        (10,0,4,0,8,8,(25,1)),(-1,0,8,0,8,8,(-125,3)),(1,0,-8,0,8,8,(-125,3)),
        (-1,0,-8,0,8,8,(125,3)),(120,2,3,2,8,8,(40,0)),
        (0,0,0,0,8,8,'FOAR0001'),(0,0,3,0,8,8,(0,0)),
        (2500,0,4,0,8,8,(625,0)),(1,0,40,0,8,8,(25,3)),
        (1,0,32,0,4,8,(3125,5)),(1,0,32,0,3,8,'result-capacity'),
        (1,0,8,0,8,2,'result-capacity'),(1,0,16,0,3,8,(625,4)),
        (1,2147483647,1,0,1,2147483647,(1,2147483647)),
        (1,0,1,2147483647,1,2147483647,'result-capacity'),
        (1,2147483647,1,2147483647,1,2147483647,(1,0)),
        (1,2,125,0,8,8,(8,5)),(10**40+8,0,8,0,48,48,((10**40+8)//8,0)),
        (1000,0,2,0,3,8,'input-capacity'),(1,3,2,0,8,2,'input-capacity')]
    for a,sa,b,sb,d,s,expected in cases:
        try:actual=divide(a,sa,b,sb,d,s)
        except (Refusal,ZeroDivisionError) as error:actual=str(error)
        assert actual==expected,(a,sa,b,sb,d,s,actual,expected)
    random=Random(9417);accepted=refused=0
    for _ in range(5000):
        a=random.randrange(-1000000,1000000);b=random.choice([1,-1])*random.randrange(1,10001)
        sa=random.randrange(0,7);sb=random.randrange(0,7)
        expected=Fraction(a,b)*Fraction(10**sb,10**sa)
        residual=expected.denominator
        while residual%2==0:residual//=2
        while residual%5==0:residual//=5
        try:
            n,s=divide(a,sa,b,sb,64,64)
            assert residual==1 and Fraction(n,10**s)==expected
            assert s==0 or n%10!=0
            accepted+=1
        except Refusal as error:
            assert str(error)=='nonterminating' and residual!=1
            refused+=1
    for _ in range(2000):
        common=random.choice([1,3,7,11]);a=common*random.randrange(-1000000,1000000)
        b=common*random.choice([1,-1])*2**random.randrange(0,16)*5**random.randrange(0,16)
        sa=random.randrange(0,9);sb=random.randrange(0,9)
        n,s=divide(a,sa,b,sb,64,64)
        assert Fraction(n,10**s)==Fraction(a,b)*Fraction(10**sb,10**sa)
        assert s==0 or n%10!=0
    for fail_at in [5,20]:
        calls=0
        def availability():
            nonlocal calls
            calls+=1
            if calls==fail_at:raise InterruptedError('cancelled')
        try:divide(1,0,5**12,0,64,64,availability)
        except InterruptedError:assert calls==fail_at
        else:raise AssertionError('factor/growth availability bypass')
    for args in [(1,0,2,0,8,8),(1,0,3,0,8,8),(1,2147483647,1,0,1,2147483647)]:
        try:divide(*args,check=lambda:(_ for _ in ()).throw(InterruptedError('cancelled')))
        except InterruptedError:pass
        else:raise AssertionError('availability bypass')
    return {'cornerCases':len(cases),'randomOracleCases':5000,'randomExactResults':accepted,'randomNonterminatingRefusals':refused,'constructedTerminatingOracleCases':2000,'entryAvailabilityCases':3,'factorOrGrowthAvailabilityCases':2,'runtimeTestsRun':False,'performanceBudgetsAccepted':False}
