# Performance Module

Chứa tất cả công cụ phân tích performance, benchmark và monitoring cho ứng dụng.

## 📁 Cấu Trúc

### Analysis Scripts
- `analyze_connectionpool_fix.py` - Phân tích hiệu quả connection pool fix
- `analyze_performance.py` - Phân tích performance tổng quát
- `analyze_system_impact.py` - Phân tích tác động hệ thống
- `analyze_tab_performance.py` - Phân tích performance theo tab

### Comparison Tools
- `compare_finetuned.py` - So sánh fine-tuned performance
- `compare_performance.py` - So sánh performance giữa các phiên bản

### Benchmark Projects
- `BenchmarkRunner.csproj` - Project chạy benchmark chính

## 🚀 Cách Sử Dụng

### 1. Phân Tích Connection Pool
```bash
python analyze_connectionpool_fix.py
```

### 2. Phân Tích Performance Tổng Quát
```bash
python analyze_performance.py
```

### 3. Phân Tích System Impact
```bash
python analyze_system_impact.py
```

### 4. Phân Tích Tab Performance
```bash
python analyze_tab_performance.py
```

### 5. So Sánh Performance
```bash
python compare_performance.py
python compare_finetuned.py
```

## 📊 Input Data

Performance scripts đọc dữ liệu từ:
- `../Logs/PerformanceAnalysis*.csv`
- `d:/temp/performance_*.csv`
- Application runtime logs

## 📈 Output Results

- Detailed analysis reports
- Performance graphs và charts
- Comparison tables
- Optimization recommendations

## 🔧 Cấu Hình

### Dependencies
```bash
pip install pandas matplotlib seaborn numpy
```

### Environment Variables
- Ensure log files accessible
- Check temp directory permissions
- Verify Python environment

## 📝 Performance Metrics

### Key Measurements
- Tab creation time
- Search operation duration
- Excel parser performance
- Memory usage patterns
- CPU utilization
- Connection pool statistics

### Performance Targets
- Tab 1-45: < 1 second
- Tab 46+: No performance cliff
- Memory: Efficient garbage collection
- CPU: Multi-core utilization

## 🔗 Integration

- **Tests Module**: Benchmark scripts tích hợp với test suite
- **Logs Module**: Analysis scripts process log data
- **Documentation**: Results documented trong performance reports
