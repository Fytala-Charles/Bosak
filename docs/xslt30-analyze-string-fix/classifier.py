"""Independent identity comparison: no aliases, description mining or phase verdicts."""
import re
STANDARD='http://www.w3.org/2005/xqt-errors'
CODE=re.compile(r'^([A-Z]{4,5}\d{4})(?=[:\s]|$)')

def expected_identity(value):
    if not value or value in ('*','XXXX9999'):return dict(kind='wildcard')
    if value.startswith('Q{') and '}' in value:
        namespace,local=value[2:].split('}',1)
        return dict(kind='qname',namespace=namespace,local=local)
    if ':' in value:return dict(kind='unresolved-prefix',raw=value)
    return dict(kind='qname',namespace=STANDARD,local=value)

def identity(error):
    if error.get('codeLocalName') and error.get('codeNamespaceUri') is not None:
        return dict(kind='qname',namespace=error['codeNamespaceUri'],local=error['codeLocalName'],origin='structured-properties')
    code=error.get('errorCode')
    if code:
        parsed=expected_identity(code)
        if code.startswith('Q{') and parsed['kind']=='qname':return dict(parsed,origin='explicit-code-property')
        if CODE.match(code):return dict(kind='reported-code',local=CODE.match(code)[1],origin='code-property',namespaceVerified=False)
        return dict(kind='uncoded',origin='unresolved-code-property',raw=code)
    message=error.get('message','')
    match=CODE.match(message)
    if match:return dict(kind='reported-code',local=match[1],origin='leading-message-code',namespaceVerified=False)
    if error.get('type')=='Bosak.XPath.Parser.XPathParseException':
        match=re.match(r'^Parse error at position -?\d+: ([A-Z]{4,5}\d{4})(?=[:\s]|$)',message)
        if match:return dict(kind='reported-code',local=match[1],origin='known-parser-wrapper',namespaceVerified=False)
    return dict(kind='uncoded',origin='no-authoritative-code')

def compare(expected,error):
    wanted=expected_identity(expected);actual=identity(error)
    if wanted['kind']=='wildcard':return 'wildcard'
    if wanted['kind']=='unresolved-prefix':return 'expected-prefix-unresolved'
    if actual['kind']=='qname':
        return 'qname-exact' if (actual['namespace'],actual['local'])==(wanted['namespace'],wanted['local']) else 'qname-mismatch'
    if actual['kind']=='reported-code':
        if actual['local']!=wanted['local']:return 'reported-code-mismatch'
        if wanted['namespace']!=STANDARD:return 'namespace-unverified'
        return 'reported-code-exact'
    return 'uncoded'

def combine(alternatives,error):
    values=[compare(code,error) for code in alternatives]
    for accepted in ['qname-exact','reported-code-exact','wildcard']:
        if accepted in values:return accepted
    for unknown in ['expected-prefix-unresolved','namespace-unverified','uncoded']:
        if unknown in values:return unknown
    return values[0] if values else 'no-error-alternative'
