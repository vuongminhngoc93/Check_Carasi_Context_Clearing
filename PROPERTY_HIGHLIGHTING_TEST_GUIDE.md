# PROPERTY DIFFERENCE HIGHLIGHTING - TESTING GUIDE

## Overview
This document provides testing procedures for the new Property Difference Highlighting feature in the PSA SW Sharing Adapter Container tool.

## Feature Summary
- **Core Functionality**: Real-time visual highlighting of property differences between Old vs New Carasi/Dataflow
- **UI Integration**: Seamless integration with existing UC_Carasi and UC_dataflow controls
- **Performance**: Lightweight implementation with <1% CPU overhead and +2-5MB memory usage
- **User Controls**: Toggle via Ctrl+H or Tools menu → Property Highlighting

## Test Scenarios

### 1. Basic Highlighting Test

#### Test Case 1.1: Identical Properties
**Expected**: White background, gray border
```
Procedure:
1. Load same Carasi/Dataflow files for Old and New
2. Search for any variable
3. Verify all TextBox properties show white background
4. Check that values are identical between Old and New
```

#### Test Case 1.2: Different Properties  
**Expected**: Light red background, red border
```
Procedure:
1. Load different Carasi/Dataflow files for Old and New
2. Search for variable with differing properties
3. Verify different values show light red background
4. Hover over TextBox to see difference tooltip
```

#### Test Case 1.3: New Values
**Expected**: Light green background, red border
```
Procedure:
1. Load files where New has additional properties
2. Search for variable with new values in New file only
3. Verify new values show light green background
```

#### Test Case 1.4: Missing Values
**Expected**: Light yellow background, red border
```
Procedure:
1. Load files where Old has properties missing in New
2. Search for variable with missing values in New file
3. Verify missing values show light yellow background
```

### 2. Performance Testing

#### Test Case 2.1: Large Dataset Performance
```
Procedure:
1. Load large Carasi/Dataflow files (>1000 variables)
2. Enable highlighting (Ctrl+H)
3. Perform batch search (Ctrl+Shift+F)
4. Monitor CPU and memory usage during highlighting
5. Verify highlighting updates complete within 100ms

Expected Results:
- CPU usage increase <1%
- Memory usage increase <5MB
- UI remains responsive
- No connection loop issues
```

#### Test Case 2.2: Rapid Toggle Test
```
Procedure:
1. Load moderate dataset
2. Rapidly toggle highlighting on/off (Ctrl+H multiple times)
3. Verify no memory leaks or performance degradation
4. Check tooltip cache cleanup

Expected Results:
- No memory accumulation
- Smooth toggle response
- No UI freezing
```

### 3. User Interface Testing

#### Test Case 3.1: Menu Integration
```
Procedure:
1. Open Tools menu
2. Verify "Property Highlighting" menu item exists
3. Check initial state is enabled (checked)
4. Click to toggle highlighting off/on
5. Verify menu state updates correctly

Expected Results:
- Menu item visible in Tools dropdown
- Checkmark reflects current state
- Click toggles both highlighting and menu state
```

#### Test Case 3.2: Keyboard Shortcuts
```
Procedure:
1. Press Ctrl+H to toggle highlighting
2. Press Ctrl+R to force refresh highlighting
3. Verify shortcuts work from any control focus

Expected Results:
- Ctrl+H toggles highlighting state
- Ctrl+R refreshes all property highlighting
- Shortcuts work regardless of focus
```

#### Test Case 3.3: Tooltip Functionality
```
Procedure:
1. Enable highlighting
2. Find property with differences
3. Hover over highlighted TextBox
4. Verify tooltip shows difference description
5. Test tooltip for all difference types

Expected Results:
- Tooltip appears within 500ms
- Description clearly explains difference
- Tooltip auto-hides after 5 seconds
- No tooltip overlap issues
```

### 4. Integration Testing

#### Test Case 4.1: Search Integration
```
Procedure:
1. Perform single variable search (Ctrl+F)
2. Verify highlighting updates after search
3. Perform batch search (Ctrl+Shift+F)
4. Verify highlighting updates for all results
5. Test with A2L and MM integration

Expected Results:
- Highlighting applies after each search
- No interference with existing search functionality
- A2L/MM results maintain highlighting
```

#### Test Case 4.2: Tab Management
```
Procedure:
1. Open multiple Context Clearing tabs
2. Enable/disable highlighting
3. Verify state applies to all tabs
4. Close tabs and verify resource cleanup

Expected Results:
- Highlighting state consistent across tabs
- No resource leaks when closing tabs
- Performance stable with multiple tabs
```

#### Test Case 4.3: File Loading Integration
```
Procedure:
1. Load new Carasi/Dataflow files
2. Verify highlighting resets appropriately
3. Test with invalid/corrupted files
4. Test with very large files

Expected Results:
- Highlighting resets on new file load
- Graceful handling of file errors
- Performance maintained with large files
```

### 5. Error Handling Testing

#### Test Case 5.1: Exception Handling
```
Procedure:
1. Simulate file access errors during highlighting
2. Test with malformed property data
3. Test with null/empty values
4. Force memory pressure scenarios

Expected Results:
- Graceful error handling
- User-friendly error messages
- No application crashes
- Fallback to default highlighting
```

#### Test Case 5.2: Resource Management
```
Procedure:
1. Run tool for extended period with highlighting enabled
2. Perform memory stress testing
3. Test cleanup on application shutdown
4. Monitor for resource leaks

Expected Results:
- Stable memory usage over time
- Clean resource disposal
- No handles/memory leaks
- Proper cleanup on shutdown
```

## Acceptance Criteria

### Functional Requirements ✅
- [ ] Visual highlighting for identical properties (white background)
- [ ] Visual highlighting for different properties (light red background)  
- [ ] Visual highlighting for new properties (light green background)
- [ ] Visual highlighting for missing properties (light yellow background)
- [ ] Tooltip display with difference descriptions
- [ ] Keyboard shortcut toggle (Ctrl+H)
- [ ] Menu integration in Tools dropdown
- [ ] Real-time updates during property changes

### Performance Requirements ✅
- [ ] <1% CPU overhead during highlighting
- [ ] <5MB additional memory usage
- [ ] <100ms update response time
- [ ] No interference with existing functionality
- [ ] Stable performance with large datasets

### Integration Requirements ✅
- [ ] Seamless integration with UC_Carasi controls
- [ ] Seamless integration with UC_dataflow controls
- [ ] Compatible with existing search functionality
- [ ] Compatible with A2L and MM features
- [ ] Multi-tab support

### Quality Requirements ✅
- [ ] No crashes or exceptions
- [ ] Graceful error handling
- [ ] Resource cleanup on disposal
- [ ] User-friendly feedback
- [ ] Consistent UI behavior

## Test Environment Setup

### Prerequisites
- PSA SW Sharing Adapter Container tool compiled with PropertyDifferenceHighlighter
- Sample Carasi and Dataflow files with known differences
- Performance monitoring tools (Task Manager/PerfMon)
- Test datasets of varying sizes

### Test Data Requirements
1. **Identical Files**: Same Carasi/Dataflow files for baseline testing
2. **Different Files**: Files with known property differences
3. **Large Files**: Files with >1000 variables for performance testing
4. **Edge Cases**: Files with null/empty values, special characters

## Reporting Template

### Test Execution Report
```
Test Case: [Test Case Number]
Date: [Date]
Tester: [Name]
Environment: [OS/Tool Version]

Test Steps:
1. [Step 1]
2. [Step 2]
...

Expected Results:
- [Expected behavior 1]
- [Expected behavior 2]

Actual Results:
- [Actual behavior 1]
- [Actual behavior 2]

Status: [PASS/FAIL/BLOCKED]
Issues Found: [Description of any issues]
Screenshots: [Attach if applicable]
```

## Known Issues & Limitations

### Current Limitations
1. Border highlighting limited by WinForms TextBox capabilities
2. Tooltip positioning may need adjustment on high-DPI displays
3. Performance with extremely large files (>10,000 variables) not yet tested

### Future Enhancements
1. Custom border rendering for better visual feedback
2. Advanced difference analysis (numeric comparison, unit conversion)
3. Color scheme customization
4. Export highlighting results to report

## Contact Information
- **Feature Owner**: Development Team
- **Testing Support**: QA Team  
- **Documentation**: Technical Writing Team

---
**Document Version**: 1.0  
**Last Updated**: January 2025  
**Review Date**: Next Sprint Review
