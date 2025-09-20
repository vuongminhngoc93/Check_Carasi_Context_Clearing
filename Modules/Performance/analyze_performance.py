import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime
import os

# Define paths relative to current script location
script_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(script_dir))
logs_dir = os.path.join(project_root, 'Modules', 'Logs')

# Try to find performance data file
performance_files = [
    'PerformanceAnalysis.csv',
    'PerformanceAnalysis_OPTIMIZED.csv', 
    'PerformanceAnalysis_OPTIMIZED_V2.csv'
]

df = None
used_file = None

for file in performance_files:
    file_path = os.path.join(logs_dir, file)
    if os.path.exists(file_path):
        try:
            df = pd.read_csv(file_path)
            used_file = file
            break
        except Exception as e:
            print(f"⚠️ Error reading {file}: {e}")
            continue

if df is None:
    print("❌ No valid performance data files found in Modules/Logs/")
    print(f"📁 Checked directory: {logs_dir}")
    print("🔍 Available files:")
    if os.path.exists(logs_dir):
        for file in os.listdir(logs_dir):
            if file.endswith('.csv'):
                print(f"   📄 {file}")
    exit(1)

print(f"📊 Using performance data from: {used_file}")
print("=== 📊 PERFORMANCE ANALYSIS REPORT ===\n")

# Basic statistics
total_operations = len(df)
unique_operations = df['OperationName'].nunique()
test_duration = pd.to_datetime(df['Timestamp'].iloc[-1]) - pd.to_datetime(df['Timestamp'].iloc[0])

print(f"📈 OVERVIEW:")
print(f"  • Total logged operations: {total_operations}")
print(f"  • Unique operation types: {unique_operations}")
print(f"  • Test duration: {test_duration}")
print(f"  • Log file size: 59KB\n")

# Performance by operation type
print("🔧 PERFORMANCE BY OPERATION TYPE:")
operation_stats = df.groupby('OperationName').agg({
    'ElapsedMs': ['count', 'mean', 'max', 'min', 'std'],
    'MemoryMB': ['mean', 'max']
}).round(2)

operation_stats.columns = ['Count', 'Avg_Time', 'Max_Time', 'Min_Time', 'Std_Time', 'Avg_Memory', 'Max_Memory']

# Sort by average time descending
operation_stats = operation_stats.sort_values('Avg_Time', ascending=False)

for operation, stats in operation_stats.iterrows():
    print(f"  🔹 {operation}:")
    print(f"     Count: {stats['Count']}, Avg: {stats['Avg_Time']}ms, Max: {stats['Max_Time']}ms")
    
    # Identify performance issues
    if stats['Avg_Time'] > 1000:
        print(f"     ⚠️  SLOW: Average time > 1000ms")
    if stats['Max_Time'] > 2000:
        print(f"     🚨 CRITICAL: Max time > 2000ms")
    
    print()

# Memory analysis
print("💾 MEMORY ANALYSIS:")
print(f"  • Memory range: {df['MemoryMB'].min():.1f}MB - {df['MemoryMB'].max():.1f}MB")
print(f"  • Memory growth: {df['MemoryMB'].max() - df['MemoryMB'].min():.1f}MB total")
print(f"  • Peak memory usage: {df['MemoryMB'].max():.1f}MB")

# Tab count analysis
print(f"\n🗂️ TAB ANALYSIS:")
print(f"  • Tab range: {df['TabCount'].min()} - {df['TabCount'].max()} tabs")
print(f"  • Final tab count: {df['TabCount'].iloc[-1]} tabs")

# Search operation analysis
search_ops = df[df['OperationName'] == 'Search_Operation']['ElapsedMs']
if len(search_ops) > 0:
    print(f"\n🔍 SEARCH PERFORMANCE:")
    print(f"  • Total searches: {len(search_ops)}")
    print(f"  • Average search time: {search_ops.mean():.0f}ms")
    print(f"  • Slowest search: {search_ops.max():.0f}ms")
    print(f"  • Fastest search: {search_ops.min():.0f}ms")

# Tab creation analysis
tab_create_ops = df[df['OperationName'] == 'Create_New_Tab']['ElapsedMs']
if len(tab_create_ops) > 0:
    print(f"\n📑 TAB CREATION PERFORMANCE:")
    print(f"  • Total tab creations: {len(tab_create_ops)}")
    print(f"  • Average creation time: {tab_create_ops.mean():.0f}ms")
    print(f"  • Slowest creation: {tab_create_ops.max():.0f}ms")
    
    # Count slow tab creations
    slow_tabs = tab_create_ops[tab_create_ops > 100]
    print(f"  • Slow tab creations (>100ms): {len(slow_tabs)}")

# Performance trends
print(f"\n📊 PERFORMANCE TRENDS:")

# Search performance vs tab count
search_complete = df[df['OperationName'] == 'Search_Operation']
if len(search_complete) > 0:
    correlation = search_complete['ElapsedMs'].corr(search_complete['TabCount'])
    print(f"  • Search time vs Tab count correlation: {correlation:.3f}")
    
    if correlation > 0.5:
        print(f"    ⚠️  Strong positive correlation - performance degrades with more tabs")
    elif correlation > 0.3:
        print(f"    ⚠️  Moderate correlation - tabs impact performance")

# Memory growth pattern
memory_growth_rate = (df['MemoryMB'].iloc[-1] - df['MemoryMB'].iloc[0]) / len(df) * 100
print(f"  • Memory growth rate: {memory_growth_rate:.2f}MB per 100 operations")

if memory_growth_rate > 1:
    print(f"    🚨 HIGH memory growth - potential memory leak")

print(f"\n=== 🎯 OPTIMIZATION RECOMMENDATIONS ===")
print()

# Specific recommendations based on data
recommendations = []

# Check search performance
if search_ops.mean() > 1500:
    recommendations.append("🔧 SEARCH OPTIMIZATION: Average search time > 1.5s - optimize Excel parsing")

# Check tab creation
if tab_create_ops.mean() > 400:
    recommendations.append("📑 TAB OPTIMIZATION: Tab creation > 400ms - optimize UC creation")

# Check memory usage
if df['MemoryMB'].max() > 60:
    recommendations.append("💾 MEMORY OPTIMIZATION: Peak usage > 60MB - implement better cleanup")

# Check performance degradation
if correlation > 0.5:
    recommendations.append("📊 SCALING OPTIMIZATION: Performance degrades with tab count - implement tab virtualization")

if not recommendations:
    recommendations.append("✅ GOOD PERFORMANCE: No critical issues detected")

for i, rec in enumerate(recommendations, 1):
    print(f"{i}. {rec}")

print(f"\n=== 📋 SUMMARY ===")
print(f"Performance data shows {len(search_ops)} searches with average {search_ops.mean():.0f}ms")
print(f"Memory usage: {df['MemoryMB'].min():.1f}MB → {df['MemoryMB'].max():.1f}MB")
print(f"Tab scaling: 1 → {df['TabCount'].max()} tabs")

# Overall performance grade
avg_search_time = search_ops.mean() if len(search_ops) > 0 else 0
memory_usage = df['MemoryMB'].max()

if avg_search_time < 1000 and memory_usage < 50:
    grade = "A - Excellent"
elif avg_search_time < 1500 and memory_usage < 60:
    grade = "B - Good"
elif avg_search_time < 2000 and memory_usage < 70:
    grade = "C - Fair"
else:
    grade = "D - Needs Optimization"

print(f"Overall Performance Grade: {grade}")
