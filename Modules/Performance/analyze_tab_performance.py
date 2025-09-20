#!/usr/bin/env python3
"""
Tab Performance Analysis: Analyze performance degradation by tab count ranges
Focus on finding performance bottlenecks at high tab counts (50+)
"""

import pandas as pd
import numpy as np
import os
from pathlib import Path

def analyze_tab_performance_ranges(filepath):
    """Analyze performance by tab count ranges"""
    try:
        df = pd.read_csv(filepath)
        print(f"🔍 TAB PERFORMANCE RANGE ANALYSIS")
        print(f"📊 Loaded {len(df)} records from {Path(filepath).name}")
        print("="*80)
        
        # Filter only COMPLETE events for operations we care about
        complete_df = df[df['EventType'] == 'COMPLETE']
        important_ops = ['Search_Operation', 'Variable_Check', 'Create_New_Tab', 'Excel_Parser_Creation']
        filtered_df = complete_df[complete_df['OperationName'].isin(important_ops)]
        
        if 'TabCount' not in filtered_df.columns:
            print("❌ TabCount column not found")
            return
        
        # Define tab count ranges
        ranges = [
            (1, 10, "🟢 Tabs 1-10"),
            (11, 20, "🟡 Tabs 11-20"), 
            (21, 30, "🟠 Tabs 21-30"),
            (31, 40, "🔴 Tabs 31-40"),
            (41, 50, "🚨 Tabs 41-50"),
            (51, 100, "💀 Tabs 51+")
        ]
        
        print(f"\n📈 PERFORMANCE BY TAB COUNT RANGES:")
        print(f"{'Range':<15} {'Count':<8} {'Avg Time':<12} {'Max Time':<12} {'Operations':<30}")
        print("-"*85)
        
        total_stats = {}
        
        for min_tabs, max_tabs, label in ranges:
            range_data = filtered_df[(filtered_df['TabCount'] >= min_tabs) & 
                                   (filtered_df['TabCount'] <= max_tabs)]
            
            if len(range_data) > 0:
                avg_time = range_data['ElapsedMs'].mean()
                max_time = range_data['ElapsedMs'].max()
                count = len(range_data)
                
                # Get operation breakdown
                op_counts = range_data['OperationName'].value_counts()
                top_ops = ', '.join([f"{op}({cnt})" for op, cnt in op_counts.head(3).items()])
                
                print(f"{label:<15} {count:<8} {avg_time:<8.0f}ms    {max_time:<8.0f}ms    {top_ops}")
                
                total_stats[label] = {
                    'avg_time': avg_time,
                    'max_time': max_time,
                    'count': count,
                    'operations': op_counts.to_dict()
                }
        
        # Detailed analysis for high tab counts (50+)
        print(f"\n🔥 DETAILED ANALYSIS: HIGH TAB COUNT PERFORMANCE (50+ tabs)")
        print("="*80)
        
        high_tab_data = filtered_df[filtered_df['TabCount'] >= 50]
        
        if len(high_tab_data) > 0:
            print(f"\n📊 Operations at 50+ tabs:")
            for operation in important_ops:
                op_data = high_tab_data[high_tab_data['OperationName'] == operation]
                if len(op_data) > 0:
                    avg_time = op_data['ElapsedMs'].mean()
                    max_time = op_data['ElapsedMs'].max()
                    count = len(op_data)
                    
                    # Performance degradation indicator
                    low_tab_data = filtered_df[(filtered_df['TabCount'] <= 10) & 
                                             (filtered_df['OperationName'] == operation)]
                    
                    if len(low_tab_data) > 0:
                        baseline_avg = low_tab_data['ElapsedMs'].mean()
                        degradation = ((avg_time - baseline_avg) / baseline_avg) * 100
                        degradation_indicator = "🚨" if degradation > 100 else "⚠️" if degradation > 50 else "✅"
                        print(f"  {degradation_indicator} {operation}: {avg_time:.0f}ms avg (vs {baseline_avg:.0f}ms baseline, +{degradation:.1f}%)")
                    else:
                        print(f"  🔹 {operation}: {avg_time:.0f}ms avg, {max_time:.0f}ms max ({count} ops)")
            
            # Memory analysis at high tab counts
            memory_data = high_tab_data['MemoryMB'].dropna()
            if len(memory_data) > 0:
                print(f"\n💾 Memory at 50+ tabs:")
                print(f"  🔹 Range: {memory_data.min():.1f}MB - {memory_data.max():.1f}MB")
                print(f"  🔹 Average: {memory_data.mean():.1f}MB")
                
            # Timeline analysis - show when performance started degrading
            print(f"\n⏱️ Timeline Analysis (50+ tabs):")
            timeline_data = high_tab_data.sort_values('TabCount')
            for _, row in timeline_data.head(10).iterrows():
                tab_count = row['TabCount']
                operation = row['OperationName']
                elapsed = row['ElapsedMs']
                memory = row.get('MemoryMB', 'N/A')
                print(f"  Tab {tab_count:2d}: {operation:<20} {elapsed:4.0f}ms (Mem: {memory}MB)")
        
        else:
            print("ℹ️ No operations found at 50+ tab count")
        
        # Performance progression analysis
        print(f"\n📈 PERFORMANCE PROGRESSION ANALYSIS:")
        print("="*80)
        
        # Group by tab count and calculate average performance
        tab_performance = []
        for tab_count in sorted(filtered_df['TabCount'].unique()):
            tab_data = filtered_df[filtered_df['TabCount'] == tab_count]
            avg_performance = tab_data['ElapsedMs'].mean()
            operation_counts = len(tab_data)
            memory_avg = tab_data['MemoryMB'].mean() if 'MemoryMB' in tab_data.columns else 0
            
            tab_performance.append({
                'tab_count': tab_count,
                'avg_performance': avg_performance,
                'operation_count': operation_counts,
                'memory': memory_avg
            })
        
        # Show performance trend
        print(f"{'Tab Count':<10} {'Avg Perf':<12} {'Ops':<6} {'Memory':<10} {'Trend':<10}")
        print("-"*55)
        
        for i, stats in enumerate(tab_performance):
            tab_count = stats['tab_count']
            avg_perf = stats['avg_performance']
            ops = stats['operation_count']
            memory = stats['memory']
            
            # Calculate trend
            if i > 0:
                prev_perf = tab_performance[i-1]['avg_performance']
                trend_pct = ((avg_perf - prev_perf) / prev_perf) * 100
                trend_icon = "📈" if trend_pct > 20 else "📉" if trend_pct < -20 else "➡️"
                trend_text = f"{trend_icon}{trend_pct:+.0f}%"
            else:
                trend_text = "---"
            
            # Highlight problematic ranges
            if tab_count >= 50:
                row_color = "🚨"
            elif tab_count >= 40:
                row_color = "⚠️"
            elif tab_count >= 30:
                row_color = "🟡"
            else:
                row_color = "🟢"
                
            print(f"{row_color} {tab_count:<7d} {avg_perf:<8.0f}ms   {ops:<4d}  {memory:<6.1f}MB   {trend_text}")
        
        return total_stats
        
    except Exception as e:
        print(f"❌ Error analyzing tab performance: {e}")
        return None

def main():
    if len(sys.argv) > 1:
        filepath = sys.argv[1]
    else:
        # Use module-based path structure
        script_dir = os.path.dirname(os.path.abspath(__file__))
        project_root = os.path.dirname(os.path.dirname(script_dir))
        logs_dir = os.path.join(project_root, 'Modules', 'Logs')
        filepath = os.path.join(logs_dir, "PerformanceAnalysis_CONNECTIONPOOL.csv")
    
    analyze_tab_performance_ranges(filepath)

if __name__ == "__main__":
    import sys
    main()
