``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-BYXUSV : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3394.0
  Job-ZAQIXM : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-GWWIVR : .NET CoreRT 1.0.27704.01 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: master @Commit: 64fca2f1b1b1bf6d5c1bbc0df3b9c55eddd63d37, 64bit AOT

Server=True  

```
|                  Method | Runtime | csvFile |       Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|------------------------ |-------- |-------- |-----------:|----------:|----------:|------:|--------:|-----------:|---------:|------:|------------:|
|      CountRowsUsingMine |     Clr |  mocked |  11.721 ms | 0.0193 ms | 0.0181 ms |  1.00 |    0.00 |          - |        - |     - |       256 B |
| CountRowsUsingCsvHelper |     Clr |  mocked | 165.300 ms | 0.5293 ms | 0.4951 ms | 14.10 |    0.05 | 18000.0000 | 333.3333 |     - | 115764283 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |    Core |  mocked |   9.996 ms | 0.0184 ms | 0.0163 ms |  1.00 |    0.00 |          - |        - |     - |       208 B |
| CountRowsUsingCsvHelper |    Core |  mocked | 178.118 ms | 0.3265 ms | 0.3054 ms | 17.82 |    0.06 |   333.3333 |        - |     - | 115757736 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |  CoreRT |  mocked |   8.753 ms | 0.0223 ms | 0.0197 ms |  1.00 |    0.00 |          - |        - |     - |       208 B |
| CountRowsUsingCsvHelper |  CoreRT |  mocked | 127.705 ms | 0.6493 ms | 0.6073 ms | 14.59 |    0.08 |   500.0000 |        - |     - | 114615166 B |
