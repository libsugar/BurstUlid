# BurstUlid

[![NPM](https://img.shields.io/npm/v/com.libsugar.sugar.unity.burstulid)](https://www.npmjs.com/package/com.libsugar.sugar.unity.burstulid)
[![openupm](https://img.shields.io/npm/v/com.libsugar.sugar.unity.burstulid?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.libsugar.sugar.unity.burstulid/)
![MIT](https://img.shields.io/github/license/libsugar/BurstUlid)

使用 Burst 实现的 Ulid 

## 安装

- Unity Package 由 [npmjs](https://www.npmjs.com/package/com.libsugar.unity.burstulid)

  如下编辑你的 `Packages/manifest.json` 文件

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

  或者在 unity 编辑器中操作  
  配置 `Project Settings -> Package Manager -> Scoped Registeries`  
  然后在包管理器中添加包  

## 提升

- 此包没有在 `package.json` 内要求依赖项,  
  你需要手动确保 ↓ 依赖项存在  

  - `Unity.Burst`
  - `Unity.Mathematics`
  - `Unity.Collections`
  - `System.Runtime.CompilerServices.Unsafe`
  - `System.Text.Json` (optional)

- 此实现不完全符合 [Ulid 标准](https://github.com/ulid/spec)

  - 没有 [单调性 (monotonicity)](https://github.com/ulid/spec#monotonicity)
  - 内存布局使用小端序

    你可以使用 `ToNetFormat` 或 `WriteNetFormatBytes` 转换成大端序  

    ```cs
    var ulid = BurstUlid.NewUlid();
    v128 net_format = ulid.ToNetFormat();
    ```

    - 内存中的二进制布局 (如果内存是小端序的)

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

    - 网络中的二进制布局 (以及 ulid 标准)

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

- 因为用了一些魔法来绕过 burst 不能调用托管代码的限制，所以如果你想在 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` 之前使用， 你需要手动调用 `InitStatic`  

  ```csharp
  BurstUlid.InitStatic();
  ```

  在一般的 unity 生命周期中使用， 比如 `Awake`、 `Start`、 `Update`， 可以忽略此条提示  

## 基准测试

![基准](https://raw.githubusercontent.com/libsugar/BurstUlid/main/benchmark.png)
