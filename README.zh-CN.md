# MongoDB with C#

[English](https://github.com/varm/dotnet.mongodb/blob/master/README.md) | 简体中文

## 简介

本项目包含一个 .NET 6 类库项目，封装了一个使用 C# 语言对 MongoDB 数据库进行 CRUD（创建, 读取, 更新, 删除） 操作的帮助类。

## 环境

个人本地测试环境，如果环境不同，请自行尝试。

* .NET 6.0
* C#/.NET Driver Version 2.17.1
* MongoDB 5.0.13

## 使用

* 打开项目 `appsettings.json` 文件，在 `mongodb` 节点中填入连接字符串，例如：

  ```
  mongodb+srv://UserName:<password>@cluster0.mongodb.net/?retryWrites=true&w=majority
  ```

  > ⚠ 请填写自己的数据库连接字符串。

* 测试项目中有 CRUD 常用操作使用示例。

## License

[MIT License](https://opensource.org/licenses/MIT) © Zerow