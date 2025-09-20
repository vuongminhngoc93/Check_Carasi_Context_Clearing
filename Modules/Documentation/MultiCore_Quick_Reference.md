# Quick Reference: Multi-Core Performance Lessons

## ⚡ **Key Insight**
Multi-core slower than single-core for Windows Forms UI-heavy small tasks = NORMAL

## 📏 **Granularity Rule** 
- Tasks < 5000ms: Single-core wins
- Tasks 800ms (tab creation): 38.4% overhead
- Tasks 1400ms (search): 7.3% overhead

## 🎯 **Architecture Decision**
Async for **UI responsiveness**, not raw speed ✅

## 📊 **Measured Results**
- BASELINE (single): 555.1ms
- CONSOLE_REMOVED (multi): 768.1ms (+38.4%)
- But: -7.3% improvement from console removal
- And: 100% batch functionality restored

## 💡 **Takeaway**
Sometimes "slower" code is better architecture when it serves user experience goals.

## 🔄 **Next Focus**
I/O optimization > more threading
