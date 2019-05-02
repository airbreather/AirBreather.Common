``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-YPPMSV : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3394.0
  Job-FSRDMI : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-BTNDUY : .NET CoreRT 1.0.27702.01 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: master @Commit: a7decabbf1e6b618040b560efcc517d6a050cf1e, 64bit AOT

Server=True  

```
|                  Method | Runtime |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |   Allocated |
|------------------------ |-------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------:|------------:|
|      CountRowsUsingMine |     Clr | 106.34 ms | 1.5686 ms | 1.4673 ms |  1.00 |    0.00 |          - |         - |     - |      1638 B |
| CountRowsUsingCsvHelper |     Clr | 570.66 ms | 7.9491 ms | 7.4356 ms |  5.37 |    0.09 | 71000.0000 | 1000.0000 |     - | 454288088 B |
|                         |         |           |           |           |       |         |            |           |       |             |
|      CountRowsUsingMine |    Core |  88.03 ms | 0.1466 ms | 0.1300 ms |  1.00 |    0.00 |          - |         - |     - |        48 B |
| CountRowsUsingCsvHelper |    Core | 608.78 ms | 1.4748 ms | 1.3073 ms |  6.92 |    0.02 |  2000.0000 |         - |     - | 454280520 B |
|                         |         |           |           |           |       |         |            |           |       |             |
|      CountRowsUsingMine |  CoreRT |  82.95 ms | 0.2999 ms | 0.2504 ms |  1.00 |    0.00 |          - |         - |     - |        48 B |
| CountRowsUsingCsvHelper |  CoreRT | 465.57 ms | 1.9902 ms | 1.7643 ms |  5.61 |    0.02 |  2000.0000 |         - |     - | 432857008 B |
