#!/bin/bash
cd /d/Development/Bosak/.sweep-baselines
for f in 372-basic-*.log; do
  set=${f#372-basic-}; set=${set%.log}
  g="373-basic-$set.log"
  [ -f "$g" ] || continue
  grep -hE "^  (PASS|FAIL)" "$f" | awk '{print $2, $1}' | sort > /tmp/b372.txt
  grep -hE "^  SKIP " "$f" | sed 's/^  SKIP //; s/:.*//' | awk '{print $1, "SKIP"}' | sort >> /tmp/b372.txt
  sort -o /tmp/b372.txt /tmp/b372.txt
  grep -hE "^  (PASS|FAIL)" "$g" | awk '{print $2, $1}' | sort > /tmp/b373.txt
  grep -hE "^  SKIP " "$g" | sed 's/^  SKIP //; s/:.*//' | awk '{print $1, "SKIP"}' | sort >> /tmp/b373.txt
  sort -o /tmp/b373.txt /tmp/b373.txt
  d=$(join /tmp/b372.txt /tmp/b373.txt | awk '$2 != $3 {print $2"->"$3}' | sort | uniq -c)
  [ -n "$d" ] && { echo "== $set"; echo "$d"; }
done
true
