#!/bin/bash
# Test script to verify presentation regeneration works

echo "=========================================="
echo "Testing Presentation Regeneration"
echo "=========================================="
echo ""

# Backup existing files
echo "1. Backing up existing presentations..."
cp Data-Access-Layer-Presentation.pptx Data-Access-Layer-Presentation.pptx.backup
cp Data-Access-Layer-Presentation.pdf Data-Access-Layer-Presentation.pdf.backup
echo "   ✅ Backup created"
echo ""

# Get original file sizes
ORIGINAL_PPTX_SIZE=$(stat -f%z Data-Access-Layer-Presentation.pptx 2>/dev/null || stat -c%s Data-Access-Layer-Presentation.pptx)
ORIGINAL_PDF_SIZE=$(stat -f%z Data-Access-Layer-Presentation.pdf 2>/dev/null || stat -c%s Data-Access-Layer-Presentation.pdf)
echo "   Original PPTX size: $ORIGINAL_PPTX_SIZE bytes"
echo "   Original PDF size: $ORIGINAL_PDF_SIZE bytes"
echo ""

# Regenerate
echo "2. Regenerating presentations..."
python3 generate_presentations.py
if [ $? -eq 0 ]; then
    echo "   ✅ Regeneration successful"
else
    echo "   ❌ Regeneration failed"
    exit 1
fi
echo ""

# Get new file sizes
NEW_PPTX_SIZE=$(stat -f%z Data-Access-Layer-Presentation.pptx 2>/dev/null || stat -c%s Data-Access-Layer-Presentation.pptx)
NEW_PDF_SIZE=$(stat -f%z Data-Access-Layer-Presentation.pdf 2>/dev/null || stat -c%s Data-Access-Layer-Presentation.pdf)
echo "   New PPTX size: $NEW_PPTX_SIZE bytes"
echo "   New PDF size: $NEW_PDF_SIZE bytes"
echo ""

# Verify
echo "3. Verifying regenerated files..."
python3 verify_presentations.py
if [ $? -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "✅ REGENERATION TEST PASSED"
    echo "=========================================="
else
    echo ""
    echo "=========================================="
    echo "❌ REGENERATION TEST FAILED"
    echo "=========================================="
    exit 1
fi

# Cleanup backups
rm -f Data-Access-Layer-Presentation.pptx.backup
rm -f Data-Access-Layer-Presentation.pdf.backup
echo ""
echo "Test completed successfully!"
