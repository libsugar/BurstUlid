# BurstUlid

[![NPM](https://img.shields.io/npm/v/com.libsugar.sugar.unity.burstulid)](https://www.npmjs.com/package/com.libsugar.sugar.unity.burstulid)
[![openupm](https://img.shields.io/npm/v/com.libsugar.sugar.unity.burstulid?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.libsugar.sugar.unity.burstulid/)
![MIT](https://img.shields.io/github/license/libsugar/BurstUlid)

Ulid implemented using burst

![ulid](https://raw.githubusercontent.com/libsugar/BurstUlid/main/ulid-logo.png)

## Installation

- Unity Package by [npmjs](https://www.npmjs.com/package/com.libsugar.unity.burstulid)

  Edit your `Packages/manifest.json` file like this

  ```json
  {
    "scopedRegistries": [
      {
        "name": "npm",
        "url": "https://registry.npmjs.org",
        "scopes": [
          "com.libsugar"
        ]
      }
    ],
    "dependencies": {
      "com.libsugar.unity.burstulid": "<version>"
    }
  }
  ```

  or use gui in unity editor  
  config `Project Settings -> Package Manager -> Scoped Registeries`  
  then add package in package manager  

## Tips

- This package does not require a dependency in `package.json`,  
  you need to manually ensure ↓ dependencies exist

  - `Unity.Burst`
  - `Unity.Mathematics`
  - `Unity.Collections`
  - `System.Runtime.CompilerServices.Unsafe`
  - `System.Text.Json` (optional)

- This implementation is not fully compliant with the [Ulid Standard](https://github.com/ulid/spec)

  - No [monotonicity](https://github.com/ulid/spec#monotonicity)
  - Memory layout uses little endian

    You can convert to big endian using `ToNetFormat` or `WriteNetFormatBytes`

    ```cs
    var ulid = BurstUlid.NewUlid();
    v128 net_format = ulid.ToNetFormat();
    ```

    - Binary layout in memory (if memory is Little-Endian)

      ```
      0                   1                   2                   3
       0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                    32_bit_uint_time_low_0123                  |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |    16_bit_uint_time_high_45    |      16_bit_uint_random_01   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                     32_bit_uint_random_2345                   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                     32_bit_uint_random_6789                   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      ```

    - Binary layout in net (and ulid stand)

      ```
      0                   1                   2                   3
       0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                    32_bit_uint_time_high_5432                 |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |    16_bit_uint_time_low_10     |      16_bit_uint_random_98   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                     32_bit_uint_random_7654                   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                     32_bit_uint_random_3210                   |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      ```

- Because some magic is used to bypass the limitation that burst cannot call managed code, so if you want to use it before `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, you need to call `InitStatic` manually

  ```csharp
  BurstUlid.InitStatic();
  ```

  Use in general unity life cycle, such as `Awake`, `Start`, `Update` can ignore this promotion

## Benchmark

![benchmark](https://raw.githubusercontent.com/libsugar/BurstUlid/main/benchmark.png)
