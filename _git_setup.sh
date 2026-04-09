#!/bin/bash
# GP MNREGA — Git helper for Cowork sessions
#
# The project is on a Windows NTFS mount which breaks git's config file.
# Work-around: clone fresh into /tmp, then use GIT_DIR + GIT_WORK_TREE.
#
# Usage (run once at start of each session):
#   source /sessions/amazing-intelligent-edison/mnt/gpmnrega-v2/gpmnrega-v2/_git_setup.sh
#
# After sourcing, normal git commands work from anywhere:
#   git status / git add / git commit / git push

REPO_URL="https://github.com/GoutamSindur1986/gpmnrega-v2.git"
WORK_TREE="/sessions/amazing-intelligent-edison/mnt/gpmnrega-v2/gpmnrega-v2"

# Write a temp global config
cat > /tmp/gitconfig <<'EOF'
[user]
    name = Goutam
    email = goutam.sindurs@gmail.com
[init]
    defaultBranch = main
EOF

# Clone bare repo into /tmp (fast – only .git objects, no checkout)
if [ ! -d /tmp/gpmnrega-git/.git ]; then
    echo "Cloning git metadata from GitHub..."
    GIT_CONFIG_GLOBAL=/tmp/gitconfig git clone --no-checkout "$REPO_URL" /tmp/gpmnrega-git 2>&1
fi

# Export the three magic vars so every subsequent git command picks them up
export GIT_DIR="$WORK_TREE/.git"
# Fall back to /tmp clone if NTFS .git is broken
if ! GIT_CONFIG_GLOBAL=/tmp/gitconfig git --git-dir="$WORK_TREE/.git" status > /dev/null 2>&1; then
    export GIT_DIR="/tmp/gpmnrega-git/.git"
fi
export GIT_WORK_TREE="$WORK_TREE"
export GIT_CONFIG_GLOBAL="/tmp/gitconfig"

echo "Git ready. GIT_DIR=$GIT_DIR"
git status --short
