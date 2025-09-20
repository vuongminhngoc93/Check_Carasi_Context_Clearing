#!/usr/bin/env python3
"""
Performance Comparison Analysis Tool
Compares optimized performance vs original performance
"""
import pandas as pd
import numpy as np
from datetime import datetime
import sys
import os

# Define paths relative to current script location
script_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(script_dir))
logs_dir = os.path.join(project_root, 'Modules', 'Logs')

def analyze_performance_improvements():
    """Compare optimized vs original performance data"""
    print("🚀 PERFORMANCE OPTIMIZATION COMPARISON ANALYSIS")
    print("=" * 60)
    
    # Read both datasets
    try:
        original_path = os.path.join(logs_dir, 'PerformanceAnalysis.csv')
        df_original = pd.read_csv(original_path)
        print(f"✅ Original data loaded: {len(df_original)} records")
    except FileNotFoundError:
        print("❌ Original performance file not found!")
        return
    
    try:
        optimized_path = os.path.join(logs_dir, 'PerformanceAnalysis_OPTIMIZED.csv')
        df_optimized = pd.read_csv(optimized_path)
        print(f"✅ Optimized data loaded: {len(df_optimized)} records")
    except FileNotFoundError:
        print("❌ Optimized performance file not found!")
        return
    
    print()
    
    # Convert timestamp to datetime
    df_original['Timestamp'] = pd.to_datetime(df_original['Timestamp'])
    df_optimized['Timestamp'] = pd.to_datetime(df_optimized['Timestamp'])
    
    # Calculate session duration
    original_duration = (df_original['Timestamp'].max() - df_original['Timestamp'].min()).total_seconds()
    optimized_duration = (df_optimized['Timestamp'].max() - df_optimized['Timestamp'].min()).total_seconds()
    
    print(f"📊 SESSION OVERVIEW:")
    print(f"   Original session: {original_duration:.0f} seconds ({original_duration/60:.1f} minutes)")
    print(f"   Optimized session: {optimized_duration:.0f} seconds ({optimized_duration/60:.1f} minutes)")
    print()
    
    # Analyze different operation types
    operations = ['Search_Operation', 'Variable_Check', 'Create_New_Tab', 'Excel_Parser_Creation', 'Excel_Parser_Disposal']
    
    print("🎯 OPERATION-BY-OPERATION COMPARISON:")
    print("-" * 60)
    
    for op in operations:
        orig_ops = df_original[df_original['OperationName'] == op]
        opt_ops = df_optimized[df_optimized['OperationName'] == op]
        
        if len(orig_ops) > 0 and len(opt_ops) > 0:
            orig_avg = orig_ops['ElapsedMs'].mean()
            opt_avg = opt_ops['ElapsedMs'].mean()
            improvement = ((orig_avg - opt_avg) / orig_avg) * 100
            
            print(f"📈 {op}:")
            print(f"   Original: {orig_avg:.0f}ms avg ({len(orig_ops)} operations)")
            print(f"   Optimized: {opt_avg:.0f}ms avg ({len(opt_ops)} operations)")
            
            if improvement > 0:
                print(f"   ✅ IMPROVEMENT: {improvement:.1f}% faster")
            else:
                print(f"   ❌ REGRESSION: {abs(improvement):.1f}% slower")
            print()
    
    # Memory analysis
    print("💾 MEMORY USAGE ANALYSIS:")
    print("-" * 60)
    
    # Get memory trends
    orig_mem_start = df_original['MemoryMB'].iloc[0] if len(df_original) > 0 else 0
    orig_mem_end = df_original['MemoryMB'].iloc[-1] if len(df_original) > 0 else 0
    orig_mem_growth = orig_mem_end - orig_mem_start
    
    opt_mem_start = df_optimized['MemoryMB'].iloc[0] if len(df_optimized) > 0 else 0
    opt_mem_end = df_optimized['MemoryMB'].iloc[-1] if len(df_optimized) > 0 else 0
    opt_mem_growth = opt_mem_end - opt_mem_start
    
    print(f"📊 Memory Growth Comparison:")
    print(f"   Original: {orig_mem_start:.1f}MB → {orig_mem_end:.1f}MB (+{orig_mem_growth:.1f}MB)")
    print(f"   Optimized: {opt_mem_start:.1f}MB → {opt_mem_end:.1f}MB (+{opt_mem_growth:.1f}MB)")
    
    mem_improvement = ((orig_mem_growth - opt_mem_growth) / orig_mem_growth) * 100 if orig_mem_growth > 0 else 0
    if mem_improvement > 0:
        print(f"   ✅ MEMORY LEAK IMPROVEMENT: {mem_improvement:.1f}% less growth")
    else:
        print(f"   ❌ MEMORY REGRESSION: {abs(mem_improvement):.1f}% more growth")
    print()
    
    # Tab count analysis
    orig_max_tabs = df_original['TabCount'].max() if len(df_original) > 0 else 0
    opt_max_tabs = df_optimized['TabCount'].max() if len(df_optimized) > 0 else 0
    
    print(f"📑 Tab Usage:")
    print(f"   Original session: Max {orig_max_tabs} tabs")
    print(f"   Optimized session: Max {opt_max_tabs} tabs")
    print()
    
    # Calculate efficiency metrics
    print("⚡ EFFICIENCY METRICS:")
    print("-" * 60)
    
    # Operations per minute
    orig_ops_per_min = len(df_original) / (original_duration / 60) if original_duration > 0 else 0
    opt_ops_per_min = len(df_optimized) / (optimized_duration / 60) if optimized_duration > 0 else 0
    
    print(f"📈 Operations per minute:")
    print(f"   Original: {orig_ops_per_min:.1f} ops/min")
    print(f"   Optimized: {opt_ops_per_min:.1f} ops/min")
    
    if opt_ops_per_min > orig_ops_per_min:
        throughput_improvement = ((opt_ops_per_min - orig_ops_per_min) / orig_ops_per_min) * 100
        print(f"   ✅ THROUGHPUT IMPROVEMENT: {throughput_improvement:.1f}% faster")
    else:
        throughput_regression = ((orig_ops_per_min - opt_ops_per_min) / orig_ops_per_min) * 100
        print(f"   ❌ THROUGHPUT REGRESSION: {throughput_regression:.1f}% slower")
    print()
    
    # Performance grade calculation
    print("🏆 PERFORMANCE GRADING:")
    print("-" * 60)
    
    def calculate_grade(avg_search_time, avg_tab_time, memory_growth_rate):
        score = 100
        
        # Search performance (target: <300ms)
        if avg_search_time > 1000:
            score -= 30
        elif avg_search_time > 600:
            score -= 20
        elif avg_search_time > 300:
            score -= 10
        
        # Tab creation (target: <200ms)
        if avg_tab_time > 500:
            score -= 20
        elif avg_tab_time > 300:
            score -= 15
        elif avg_tab_time > 200:
            score -= 10
        
        # Memory growth (target: <3MB per 100 ops)
        if memory_growth_rate > 10:
            score -= 30
        elif memory_growth_rate > 7:
            score -= 20
        elif memory_growth_rate > 3:
            score -= 10
        
        return max(0, score)
    
    # Calculate grades for both
    orig_search_avg = df_original[df_original['OperationName'] == 'Variable_Check']['ElapsedMs'].mean() if len(df_original[df_original['OperationName'] == 'Variable_Check']) > 0 else 0
    orig_tab_avg = df_original[df_original['OperationName'] == 'Create_New_Tab']['ElapsedMs'].mean() if len(df_original[df_original['OperationName'] == 'Create_New_Tab']) > 0 else 0
    orig_mem_rate = (orig_mem_growth / len(df_original)) * 100 if len(df_original) > 0 else 0
    
    opt_search_avg = df_optimized[df_optimized['OperationName'] == 'Variable_Check']['ElapsedMs'].mean() if len(df_optimized[df_optimized['OperationName'] == 'Variable_Check']) > 0 else 0
    opt_tab_avg = df_optimized[df_optimized['OperationName'] == 'Create_New_Tab']['ElapsedMs'].mean() if len(df_optimized[df_optimized['OperationName'] == 'Create_New_Tab']) > 0 else 0
    opt_mem_rate = (opt_mem_growth / len(df_optimized)) * 100 if len(df_optimized) > 0 else 0
    
    orig_grade = calculate_grade(orig_search_avg, orig_tab_avg, orig_mem_rate)
    opt_grade = calculate_grade(opt_search_avg, opt_tab_avg, opt_mem_rate)
    
    def grade_to_letter(score):
        if score >= 90: return "A-Excellent"
        elif score >= 80: return "B-Good"
        elif score >= 70: return "C-Fair"
        elif score >= 60: return "D-Poor"
        else: return "F-Critical"
    
    print(f"📊 Original Performance: {orig_grade}/100 - {grade_to_letter(orig_grade)}")
    print(f"📊 Optimized Performance: {opt_grade}/100 - {grade_to_letter(opt_grade)}")
    
    grade_improvement = opt_grade - orig_grade
    if grade_improvement > 0:
        print(f"   ✅ GRADE IMPROVEMENT: +{grade_improvement} points")
    else:
        print(f"   ❌ GRADE REGRESSION: {grade_improvement} points")
    
    print()
    print("🎯 OPTIMIZATION IMPACT SUMMARY:")
    print("-" * 60)
    
    # Key metrics summary
    search_improvement = ((orig_search_avg - opt_search_avg) / orig_search_avg) * 100 if orig_search_avg > 0 else 0
    tab_improvement = ((orig_tab_avg - opt_tab_avg) / orig_tab_avg) * 100 if orig_tab_avg > 0 else 0
    
    if search_improvement > 0:
        print(f"✅ Search operations: {search_improvement:.1f}% faster")
    else:
        print(f"❌ Search operations: {abs(search_improvement):.1f}% slower")
    
    if tab_improvement > 0:
        print(f"✅ Tab creation: {tab_improvement:.1f}% faster")
    else:
        print(f"❌ Tab creation: {abs(tab_improvement):.1f}% slower")
    
    if mem_improvement > 0:
        print(f"✅ Memory usage: {mem_improvement:.1f}% less growth")
    else:
        print(f"❌ Memory usage: {abs(mem_improvement):.1f}% more growth")
    
    print(f"🏆 Overall grade: {grade_to_letter(orig_grade)} → {grade_to_letter(opt_grade)}")
    
    if opt_grade > orig_grade:
        print(f"🚀 OPTIMIZATION SUCCESS! Performance improved by {grade_improvement} points")
    else:
        print(f"⚠️  OPTIMIZATION REGRESSION! Performance decreased by {abs(grade_improvement)} points")

if __name__ == "__main__":
    analyze_performance_improvements()
