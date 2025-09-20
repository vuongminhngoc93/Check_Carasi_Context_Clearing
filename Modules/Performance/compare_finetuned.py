#!/usr/bin/env python3
"""
Performance Comparison: Original vs Optimized vs Fine-tuned
Compare three versions to track optimization progress
"""

import pandas as pd
import sys
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

def analyze_operations(df, version_name):
    """Analyze performance by operation type"""
    print(f"\n🔧 {version_name} PERFORMANCE:")
    
    results = {}
    
    # Filter only COMPLETE events and group by OperationName
    complete_df = df[df['EventType'] == 'COMPLETE']
    operation_groups = complete_df.groupby('OperationName')
    
    for operation, group in operation_groups:
        if operation in ['Search_Operation', 'Variable_Check', 'Create_New_Tab', 'Excel_Parser_Creation']:
            avg_time = group['ElapsedMs'].mean()
            max_time = group['ElapsedMs'].max()
            count = len(group)
            
            results[operation] = {
                'avg': avg_time,
                'max': max_time,
                'count': count
            }
            
            print(f"  🔹 {operation}: {avg_time:.1f}ms avg, {max_time:.0f}ms max ({count} ops)")
    
    return results

def calculate_improvement(original, new):
    """Calculate improvement percentage"""
    if original == 0:
        return 0
    return ((original - new) / original) * 100

def compare_versions(original_results, optimized_results, finetuned_results):
    """Compare all three versions"""
    print(f"\n🔄 PERFORMANCE COMPARISON:")
    print(f"{'Operation':<20} {'Original':<12} {'Optimized':<12} {'Fine-tuned':<12} {'Opt Change':<12} {'Final Change':<12}")
    print("="*90)
    
    total_original = 0
    total_optimized = 0
    total_finetuned = 0
    
    for operation in ['Search_Operation', 'Variable_Check', 'Create_New_Tab', 'Excel_Parser_Creation']:
        if operation in original_results and operation in optimized_results and operation in finetuned_results:
            orig_avg = original_results[operation]['avg']
            opt_avg = optimized_results[operation]['avg']
            fine_avg = finetuned_results[operation]['avg']
            
            opt_change = calculate_improvement(orig_avg, opt_avg)
            final_change = calculate_improvement(orig_avg, fine_avg)
            
            # Format with color indicators
            opt_indicator = "🟢" if opt_change > 0 else "🔴" if opt_change < -5 else "🟡"
            final_indicator = "🟢" if final_change > 0 else "🔴" if final_change < -5 else "🟡"
            
            print(f"{operation:<20} {orig_avg:<8.1f}ms {opt_avg:<8.1f}ms {fine_avg:<8.1f}ms {opt_indicator}{opt_change:+6.1f}% {final_indicator}{final_change:+6.1f}%")
            
            total_original += orig_avg
            total_optimized += opt_avg
            total_finetuned += fine_avg
    
    print("-"*90)
    total_opt_change = calculate_improvement(total_original, total_optimized)
    total_final_change = calculate_improvement(total_original, total_finetuned)
    
    opt_indicator = "🟢" if total_opt_change > 0 else "🔴"
    final_indicator = "🟢" if total_final_change > 0 else "🔴"
    
    print(f"{'TOTAL':<20} {total_original:<8.1f}ms {total_optimized:<8.1f}ms {total_finetuned:<8.1f}ms {opt_indicator}{total_opt_change:+6.1f}% {final_indicator}{total_final_change:+6.1f}%")
    
    return total_original, total_optimized, total_finetuned

def analyze_memory(df, version_name):
    """Analyze memory usage"""
    if 'MemoryMB' in df.columns:
        memory_data = df['MemoryMB'].dropna()
        if len(memory_data) > 0:
            min_mem = memory_data.min()
            max_mem = memory_data.max()
            growth = max_mem - min_mem
            print(f"  💾 {version_name} Memory: {min_mem:.1f}MB → {max_mem:.1f}MB (Growth: {growth:.1f}MB)")
            return {'min': min_mem, 'max': max_mem, 'growth': growth}
    return None

def main():
    print("🔍 PERFORMANCE COMPARISON: Original vs Optimized vs Fine-tuned")
    print("="*70)
    
    # Load all three datasets using module structure
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(os.path.dirname(script_dir))
    logs_dir = os.path.join(project_root, 'Modules', 'Logs')
    
    original_file = os.path.join(logs_dir, "PerformanceAnalysis.csv")
    optimized_file = os.path.join(logs_dir, "PerformanceAnalysis_OPTIMIZED.csv")
    finetuned_file = os.path.join(logs_dir, "PerformanceAnalysis_FINETUNED.csv")
    
    df_original = load_performance_data(original_file)
    df_optimized = load_performance_data(optimized_file)
    df_finetuned = load_performance_data(finetuned_file)
    
    if df_original is None or df_optimized is None or df_finetuned is None:
        print("❌ Cannot load all required files")
        return
    
    # Analyze each version
    original_results = analyze_operations(df_original, "ORIGINAL")
    optimized_results = analyze_operations(df_optimized, "OPTIMIZED") 
    finetuned_results = analyze_operations(df_finetuned, "FINE-TUNED")
    
    # Compare all versions
    total_original, total_optimized, total_finetuned = compare_versions(original_results, optimized_results, finetuned_results)
    
    # Calculate overall changes
    total_opt_change = calculate_improvement(total_original, total_optimized)
    total_final_change = calculate_improvement(total_original, total_finetuned)
    final_indicator = "🟢" if total_final_change > 0 else "🔴"
    
    # Memory comparison
    print(f"\n💾 MEMORY COMPARISON:")
    orig_mem = analyze_memory(df_original, "Original")
    opt_mem = analyze_memory(df_optimized, "Optimized")
    fine_mem = analyze_memory(df_finetuned, "Fine-tuned")
    
    if orig_mem and opt_mem and fine_mem:
        opt_mem_change = calculate_improvement(orig_mem['growth'], opt_mem['growth'])
        final_mem_change = calculate_improvement(orig_mem['growth'], fine_mem['growth'])
        
        print(f"\n📊 MEMORY IMPROVEMENT:")
        print(f"  🔹 Optimized version: {opt_mem_change:+.1f}% memory growth change")
        print(f"  🔹 Fine-tuned version: {final_mem_change:+.1f}% memory growth change")
    
    print(f"\n🎯 CONCLUSION:")
    print(f"  Fine-tuning has {'improved' if total_final_change > total_opt_change else 'maintained'} the optimization results")
    print(f"  Overall performance change from original: {final_indicator}{total_final_change:+.1f}%")

if __name__ == "__main__":
    main()
