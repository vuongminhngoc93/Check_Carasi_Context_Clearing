#!/usr/bin/env python3
"""
System Resource Analysis: Check if system overload affects performance
Analyze system resources during performance tests
"""

import pandas as pd
import psutil
import os
import time
from pathlib import Path

def get_current_system_status():
    """Get current system resource usage"""
    print("🖥️ CURRENT SYSTEM STATUS:")
    print("="*50)
    
    # CPU Usage
    cpu_percent = psutil.cpu_percent(interval=1)
    cpu_count = psutil.cpu_count()
    cpu_freq = psutil.cpu_freq()
    
    print(f"🔧 CPU:")
    print(f"  Usage: {cpu_percent:.1f}%")
    print(f"  Cores: {cpu_count}")
    if cpu_freq:
        print(f"  Frequency: {cpu_freq.current:.0f} MHz")
    
    # Memory Usage
    memory = psutil.virtual_memory()
    print(f"\n💾 MEMORY:")
    print(f"  Total: {memory.total / (1024**3):.1f} GB")
    print(f"  Available: {memory.available / (1024**3):.1f} GB")
    print(f"  Used: {memory.percent:.1f}%")
    
    # Disk Usage
    disk = psutil.disk_usage('/')
    print(f"\n💽 DISK:")
    print(f"  Total: {disk.total / (1024**3):.1f} GB")
    print(f"  Free: {disk.free / (1024**3):.1f} GB")
    print(f"  Used: {disk.percent:.1f}%")
    
    # Network
    network = psutil.net_io_counters()
    print(f"\n🌐 NETWORK:")
    print(f"  Bytes sent: {network.bytes_sent / (1024**2):.1f} MB")
    print(f"  Bytes received: {network.bytes_recv / (1024**2):.1f} MB")
    
    # Running processes count
    processes = len(psutil.pids())
    print(f"\n⚙️ PROCESSES: {processes} running")
    
    return {
        'cpu_percent': cpu_percent,
        'memory_percent': memory.percent,
        'disk_percent': disk.percent,
        'process_count': processes
    }

def analyze_performance_with_system_context(df, version_name):
    """Analyze performance data with system resource context"""
    print(f"\n📊 {version_name} PERFORMANCE WITH SYSTEM CONTEXT:")
    print("="*60)
    
    # Check if we have memory data in performance log
    if 'MemoryMB' in df.columns:
        memory_data = df['MemoryMB'].dropna()
        if len(memory_data) > 0:
            min_mem = memory_data.min()
            max_mem = memory_data.max()
            growth = max_mem - min_mem
            
            print(f"📈 APPLICATION MEMORY USAGE:")
            print(f"  Start: {min_mem:.1f} MB")
            print(f"  Peak: {max_mem:.1f} MB")
            print(f"  Growth: {growth:.1f} MB")
            
            # Memory growth rate analysis
            if growth > 500:  # > 500MB growth
                print(f"  ⚠️  High memory growth detected")
            elif growth > 200:
                print(f"  🟡 Moderate memory growth")
            else:
                print(f"  ✅ Normal memory growth")
    
    # Analyze operation performance patterns
    complete_events = df[df['EventType'] == 'COMPLETE']
    
    if len(complete_events) > 0:
        # Group by operation and analyze patterns
        operations = ['Create_New_Tab', 'Excel_Parser_Creation', 'Search_Operation', 'Variable_Check']
        
        print(f"\n🔧 OPERATION PERFORMANCE PATTERNS:")
        
        for operation in operations:
            op_data = complete_events[complete_events['OperationName'] == operation]
            if len(op_data) > 0:
                times = op_data['ElapsedMs'].values
                avg_time = times.mean()
                std_time = times.std()
                
                # Check for performance degradation over time
                if len(times) > 10:
                    first_10 = times[:10].mean()
                    last_10 = times[-10:].mean()
                    degradation = ((last_10 - first_10) / first_10) * 100
                    
                    if degradation > 50:
                        trend = "🔴 DEGRADING"
                    elif degradation > 20:
                        trend = "🟡 SLIGHT DECLINE"
                    elif degradation < -20:
                        trend = "🟢 IMPROVING"
                    else:
                        trend = "➡️ STABLE"
                    
                    print(f"  {operation}:")
                    print(f"    Average: {avg_time:.1f}ms (±{std_time:.1f}ms)")
                    print(f"    Trend: {trend} ({degradation:+.1f}%)")

def compare_system_impact():
    """Compare different test sessions to see system impact"""
    print("\n🔄 SYSTEM IMPACT ANALYSIS:")
    print("="*50)
    
    # File paths using module structure
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(os.path.dirname(script_dir))
    logs_dir = os.path.join(project_root, 'Modules', 'Logs')
    
    files = [
        (os.path.join(logs_dir, "PerformanceAnalysis.csv"), "ORIGINAL"),
        (os.path.join(logs_dir, "PerformanceAnalysis_OPTIMIZED.csv"), "OPTIMIZED"),
        (os.path.join(logs_dir, "PerformanceAnalysis_FINETUNED.csv"), "FINE-TUNED"),
        (os.path.join(logs_dir, "PerformanceAnalysis_CONNECTIONPOOL_FIX.csv"), "CONNECTION POOL FIX")
    ]
    
    system_context = {}
    
    for filename, version in files:
        filepath = f"d:\\5_Automation\\Check_carasi_DF_ContextClearing\\{filename}"
        if Path(filepath).exists():
            try:
                df = pd.read_csv(filepath)
                
                # Calculate performance metrics
                tab_events = df[df['OperationName'] == 'Create_New_Tab']
                parser_events = df[df['OperationName'] == 'Excel_Parser_Creation']
                
                metrics = {
                    'tab_avg': tab_events['ElapsedMs'].mean() if len(tab_events) > 0 else 0,
                    'parser_avg': parser_events['ElapsedMs'].mean() if len(parser_events) > 0 else 0,
                    'total_operations': len(df),
                    'test_duration': 0  # We could calculate this from timestamps
                }
                
                if 'MemoryMB' in df.columns:
                    memory_data = df['MemoryMB'].dropna()
                    if len(memory_data) > 0:
                        metrics['memory_growth'] = memory_data.max() - memory_data.min()
                
                system_context[version] = metrics
                
            except Exception as e:
                print(f"  ❌ Error loading {filename}: {e}")
    
    # Analyze patterns across versions
    if len(system_context) > 1:
        print(f"\n📈 PERFORMANCE CONSISTENCY ANALYSIS:")
        
        versions = list(system_context.keys())
        for metric in ['tab_avg', 'parser_avg']:
            print(f"\n  {metric.upper()}:")
            values = [system_context[v].get(metric, 0) for v in versions]
            
            # Check for consistency
            if max(values) > 0:
                variation = (max(values) - min(values)) / max(values) * 100
                
                if variation > 100:
                    consistency = "🔴 HIGH VARIATION"
                elif variation > 50:
                    consistency = "🟡 MODERATE VARIATION"
                else:
                    consistency = "✅ CONSISTENT"
                
                print(f"    Variation: {variation:.1f}% - {consistency}")
                
                for i, version in enumerate(versions):
                    print(f"    {version}: {values[i]:.1f}ms")

def check_background_processes():
    """Check for resource-intensive background processes"""
    print(f"\n⚙️ BACKGROUND PROCESS ANALYSIS:")
    print("="*50)
    
    # Get top CPU consuming processes
    processes = []
    for proc in psutil.process_iter(['pid', 'name', 'cpu_percent', 'memory_percent']):
        try:
            proc_info = proc.info
            if proc_info['cpu_percent'] > 5 or proc_info['memory_percent'] > 5:
                processes.append(proc_info)
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            pass
    
    # Sort by CPU usage
    processes.sort(key=lambda x: x['cpu_percent'], reverse=True)
    
    print(f"🔧 HIGH RESOURCE PROCESSES (CPU > 5% OR Memory > 5%):")
    if processes:
        for proc in processes[:10]:  # Top 10
            print(f"  {proc['name']} (PID: {proc['pid']}): CPU {proc['cpu_percent']:.1f}%, Memory {proc['memory_percent']:.1f}%")
    else:
        print("  ✅ No high resource processes detected")

def main():
    print("🖥️ SYSTEM RESOURCE IMPACT ANALYSIS")
    print("="*60)
    print("Checking if system overload affects application performance")
    
    # Get current system status
    current_status = get_current_system_status()
    
    # Check background processes
    check_background_processes()
    
    # Analyze performance data with system context
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(os.path.dirname(script_dir))
    logs_dir = os.path.join(project_root, 'Modules', 'Logs')
    latest_file = os.path.join(logs_dir, "PerformanceAnalysis_CONNECTIONPOOL_FIX.csv")
    if Path(latest_file).exists():
        try:
            df = pd.read_csv(latest_file)
            analyze_performance_with_system_context(df, "LATEST TEST")
        except Exception as e:
            print(f"❌ Error loading latest performance data: {e}")
    
    # Compare system impact across versions
    compare_system_impact()
    
    # Recommendations
    print(f"\n🎯 SYSTEM PERFORMANCE RECOMMENDATIONS:")
    print("="*50)
    
    if current_status['cpu_percent'] > 80:
        print("  🔴 High CPU usage detected - Consider closing unnecessary applications")
    
    if current_status['memory_percent'] > 85:
        print("  🔴 High memory usage detected - System may be swapping to disk")
    
    if current_status['process_count'] > 200:
        print("  🟡 High process count - System may be overloaded")
    
    if (current_status['cpu_percent'] < 50 and 
        current_status['memory_percent'] < 70 and 
        current_status['process_count'] < 150):
        print("  ✅ System resources look healthy")
        print("  ➡️ Performance issues likely application-specific, not system overload")
    else:
        print("  ⚠️ System may be under stress - This could affect application performance")

if __name__ == "__main__":
    main()
