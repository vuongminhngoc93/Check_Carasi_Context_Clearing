#!/usr/bin/env python3
"""
Connection Pool Fix Analysis: Focus on Tab 40+ Performance
Analyze if the connection pool architecture fix resolved the tab 40+ slowdown
"""

import pandas as pd
import matplotlib.pyplot as plt
import os
from pathlib import Path

def load_performance_data(filepath):
    """Load and prepare performance data"""
    try:
        df = pd.read_csv(filepath)
        print(f"✅ Loaded {len(df)} records from {Path(filepath).name}")
        return df
    except Exception as e:
        print(f"❌ Error loading {filepath}: {e}")
        return None

def analyze_tab_performance(df, version_name):
    """Analyze performance by tab count focusing on tab 40+"""
    print(f"\n📊 {version_name} TAB PERFORMANCE ANALYSIS:")
    
    # Filter CREATE_NEW_TAB events and extract tab numbers
    tab_events = df[df['OperationName'] == 'Create_New_Tab'].copy()
    
    if len(tab_events) == 0:
        print("  ❌ No tab creation events found")
        return None
    
    # Extract tab numbers from OperationDescription if available
    tab_events['TabNumber'] = range(1, len(tab_events) + 1)
    
    # Group by tab ranges
    ranges = [
        (1, 20, "Tabs 1-20"),
        (21, 40, "Tabs 21-40"), 
        (41, 50, "Tabs 41-50"),
        (51, 60, "Tabs 51-60"),
        (61, 100, "Tabs 61+")
    ]
    
    results = {}
    
    for start, end, label in ranges:
        range_data = tab_events[(tab_events['TabNumber'] >= start) & (tab_events['TabNumber'] <= end)]
        if len(range_data) > 0:
            avg_time = range_data['ElapsedMs'].mean()
            max_time = range_data['ElapsedMs'].max()
            count = len(range_data)
            
            results[label] = {
                'avg': avg_time,
                'max': max_time,
                'count': count,
                'range': (start, end)
            }
            
            # Color coding for performance
            if avg_time > 5000:  # > 5 seconds
                status = "🔴 SLOW"
            elif avg_time > 2000:  # > 2 seconds  
                status = "🟡 MODERATE"
            else:
                status = "🟢 FAST"
            
            print(f"  {status} {label}: {avg_time:.0f}ms avg, {max_time:.0f}ms max ({count} tabs)")
    
    return results

def analyze_excel_parser_performance(df, version_name):
    """Analyze Excel_Parser_Creation performance - key indicator of connection issues"""
    print(f"\n🔧 {version_name} EXCEL PARSER PERFORMANCE:")
    
    parser_events = df[df['OperationName'] == 'Excel_Parser_Creation']
    
    if len(parser_events) == 0:
        print("  ❌ No Excel Parser creation events found")
        return None
    
    avg_time = parser_events['ElapsedMs'].mean()
    max_time = parser_events['ElapsedMs'].max()
    min_time = parser_events['ElapsedMs'].min()
    count = len(parser_events)
    
    # Performance indicators
    if avg_time > 3000:  # > 3 seconds indicates connection issues
        status = "🔴 CONNECTION ISSUES"
    elif avg_time > 1000:  # > 1 second
        status = "🟡 SLOW"
    else:
        status = "🟢 HEALTHY"
    
    print(f"  {status} Excel Parser: {avg_time:.0f}ms avg, {max_time:.0f}ms max, {min_time:.0f}ms min ({count} ops)")
    
    return {
        'avg': avg_time,
        'max': max_time, 
        'min': min_time,
        'count': count
    }

def compare_with_previous(old_file, new_file):
    """Compare performance between old problematic version and new fix"""
    print("\n🔄 COMPARISON: Before vs After Connection Pool Fix")
    print("="*80)
    
    df_old = load_performance_data(old_file)
    df_new = load_performance_data(new_file)
    
    if df_old is None or df_new is None:
        print("❌ Cannot load comparison files")
        return
    
    print(f"\n📈 TAB PERFORMANCE COMPARISON:")
    old_tabs = analyze_tab_performance(df_old, "BEFORE (With Double Pooling)")
    new_tabs = analyze_tab_performance(df_new, "AFTER (Single Pool Fix)")
    
    if old_tabs and new_tabs:
        print(f"\n📊 TAB RANGE IMPROVEMENTS:")
        for range_name in old_tabs.keys():
            if range_name in new_tabs:
                old_avg = old_tabs[range_name]['avg']
                new_avg = new_tabs[range_name]['avg']
                improvement = ((old_avg - new_avg) / old_avg) * 100
                
                if improvement > 20:
                    indicator = "🟢 MAJOR IMPROVEMENT"
                elif improvement > 0:
                    indicator = "🟢 IMPROVEMENT"
                elif improvement > -20:
                    indicator = "🟡 SIMILAR"
                else:
                    indicator = "🔴 REGRESSION"
                
                print(f"  {indicator} {range_name}: {old_avg:.0f}ms → {new_avg:.0f}ms ({improvement:+.1f}%)")
    
    print(f"\n🔧 EXCEL PARSER COMPARISON:")
    old_parser = analyze_excel_parser_performance(df_old, "BEFORE")
    new_parser = analyze_excel_parser_performance(df_new, "AFTER")
    
    if old_parser and new_parser:
        parser_improvement = ((old_parser['avg'] - new_parser['avg']) / old_parser['avg']) * 100
        
        if parser_improvement > 50:
            indicator = "🟢 MAJOR FIX"
        elif parser_improvement > 0:
            indicator = "🟢 IMPROVED"
        else:
            indicator = "🔴 NO IMPROVEMENT"
        
        print(f"  {indicator} Parser Creation: {old_parser['avg']:.0f}ms → {new_parser['avg']:.0f}ms ({parser_improvement:+.1f}%)")

def main():
    print("🔍 CONNECTION POOL FIX ANALYSIS")
    print("="*50)
    print("Focus: Tab 40+ performance and connection pool exhaustion")
    
    # Analyze the new connection pool fix version using module structure
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(os.path.dirname(script_dir))
    logs_dir = os.path.join(project_root, 'Modules', 'Logs')
    
    new_file = os.path.join(logs_dir, "PerformanceAnalysis_CONNECTIONPOOL_FIX.csv")
    df_new = load_performance_data(new_file)
    
    if df_new is None:
        print("❌ Cannot load connection pool fix performance data")
        return
    
    # Analyze current performance
    tab_results = analyze_tab_performance(df_new, "CONNECTION POOL FIX")
    parser_results = analyze_excel_parser_performance(df_new, "CONNECTION POOL FIX")
    
    # Compare with previous problematic version (FINETUNED had the connection issues)
    comparison_file = os.path.join(logs_dir, "PerformanceAnalysis_FINETUNED.csv")
    if Path(comparison_file).exists():
        compare_with_previous(comparison_file, new_file)
    
    print(f"\n🎯 CONNECTION POOL FIX ASSESSMENT:")
    
    # Check if tab 40+ issues are resolved
    if tab_results:
        tab_40_plus = [v for k, v in tab_results.items() if "41-" in k or "51-" in k or "61+" in k]
        if tab_40_plus:
            avg_high_tab_time = sum(r['avg'] for r in tab_40_plus) / len(tab_40_plus)
            if avg_high_tab_time < 3000:  # Less than 3 seconds
                print("  ✅ Tab 40+ performance: RESOLVED - No more severe slowdown")
            else:
                print("  ❌ Tab 40+ performance: STILL SLOW - Connection issues persist")
        else:
            print("  ⚠️  Not enough high tab data to assess")
    
    # Check if Excel Parser creation is healthy
    if parser_results:
        if parser_results['avg'] < 1000:  # Less than 1 second
            print("  ✅ Excel Parser creation: HEALTHY - No connection pool exhaustion")
        else:
            print("  ❌ Excel Parser creation: SLOW - Connection issues persist")

if __name__ == "__main__":
    main()
