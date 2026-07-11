#!/usr/bin/env sh
#
# phase7.sh — the definition of done for the PrimeNG -> spartan/ui migration.
#
#   sh tools/gates/phase7.sh
#
# Exit 0 = PrimeNG is gone and stays gone. Exit 1 = it came back.
#
# ---------------------------------------------------------------------------------------------
# READ THIS BEFORE "FIXING" A FAILURE BY DELETING A COMMENT.
#
# The gate greps for PrimeNG in CODE, not in PROSE. ~75 files carry comments that NAME PrimeNG in
# order to explain what was removed and why -- those are exempt, deliberately, and they are the most
# valuable prose in the repo. If this gate fails, a real `<p-button>` / `pTooltip` / `.p-card` /
# `FormsModule` is back in shipped code. Remove the CODE.
#
# The scan itself lives in phase7.mjs, which explains at length why it is an INDEPENDENT scanner and
# not a reuse of burndown.mjs. Short version: the migration's most expensive bug was a checker that
# shared a scanner with the thing it checked, and so confirmed only its own blind spots.
# ---------------------------------------------------------------------------------------------

set -eu

cd "$(dirname "$0")/../.."   # -> src/Client/Logistics.Angular

echo "==============================================================================="
echo " PHASE 7 EXIT GATE"
echo "==============================================================================="

# 1. The scan: source (comment-aware, binary-safe) + package.json + bun.lock + angular.json.
#    Self-tests its own comment stripper first -- a gate that has not proved it can see is not a gate.
node tools/gates/phase7.mjs

# 2. The migration ratchet, run as a SECOND, INDEPENDENT scanner over the same question.
#    burndown.mjs walks the tree via `git ls-files` and strips comments with a different algorithm.
#    Two independent implementations agreeing on 0 is evidence; one agreeing with itself is not.
#    (This also runs budgets, spartan-tokens and icons.)
echo ""
echo "--- cross-check: check:all (burndown = independent scanner, + budgets/tokens/icons) ---"
node tools/checks/check-all.mjs

echo ""
echo "==============================================================================="
echo " PHASE 7: PASS -- PrimeNG is gone."
echo "==============================================================================="
