#!/usr/bin/env python3
"""Verification script for presentation files"""

import os
import sys
from pptx import Presentation

def verify_powerpoint():
    """Verify PowerPoint file"""
    filename = 'Data-Access-Layer-Presentation.pptx'
    
    if not os.path.exists(filename):
        print(f"❌ File not found: {filename}")
        return False
    
    try:
        prs = Presentation(filename)
        num_slides = len(prs.slides)
        
        print(f"✅ PowerPoint verification:")
        print(f"   - File: {filename}")
        print(f"   - Size: {os.path.getsize(filename) / 1024:.1f} KB")
        print(f"   - Slides: {num_slides}")
        
        if num_slides < 10:
            print(f"   ⚠️  Warning: Only {num_slides} slides found")
            return False
        
        return True
    except Exception as e:
        print(f"❌ Error opening PowerPoint: {e}")
        return False

def verify_pdf():
    """Verify PDF file"""
    filename = 'Data-Access-Layer-Presentation.pdf'
    
    if not os.path.exists(filename):
        print(f"❌ File not found: {filename}")
        return False
    
    try:
        size = os.path.getsize(filename)
        
        print(f"✅ PDF verification:")
        print(f"   - File: {filename}")
        print(f"   - Size: {size / 1024:.1f} KB")
        
        # Basic validation
        with open(filename, 'rb') as f:
            header = f.read(5)
            if header != b'%PDF-':
                print(f"   ❌ Invalid PDF header")
                return False
        
        return True
    except Exception as e:
        print(f"❌ Error opening PDF: {e}")
        return False

def verify_documentation():
    """Verify documentation file"""
    filename = 'PRESENTATION-README.md'
    
    if not os.path.exists(filename):
        print(f"❌ File not found: {filename}")
        return False
    
    try:
        with open(filename, 'r', encoding='utf-8') as f:
            content = f.read()
        
        print(f"✅ Documentation verification:")
        print(f"   - File: {filename}")
        print(f"   - Size: {len(content)} characters")
        
        # Check for key sections
        required_sections = ['File Presentasi', 'Cara Menggunakan', 'Regenerate']
        missing = [s for s in required_sections if s not in content]
        
        if missing:
            print(f"   ⚠️  Missing sections: {', '.join(missing)}")
            return False
        
        return True
    except Exception as e:
        print(f"❌ Error reading documentation: {e}")
        return False

def main():
    """Main verification"""
    print("=" * 60)
    print("PRESENTATION FILES VERIFICATION")
    print("=" * 60)
    print()
    
    results = []
    results.append(verify_powerpoint())
    print()
    results.append(verify_pdf())
    print()
    results.append(verify_documentation())
    
    print()
    print("=" * 60)
    
    if all(results):
        print("✅ ALL CHECKS PASSED")
        print("=" * 60)
        return 0
    else:
        print("❌ SOME CHECKS FAILED")
        print("=" * 60)
        return 1

if __name__ == '__main__':
    sys.exit(main())
