# MongoDB with C#

English | [简体中文](https://github.com/varm/dotnet.mongodb/blob/master/README.zh-CN.md)

## Introduction 

This project contains a .NET 6 class library that encapsulates a helper class for CRUD (Create, Read, Update, Delete) operations on a MongoDB database using the C# language.

## Development

The following is a personal local development environment, if the environment is different, please try it yourself.

* Visual Studio 2022

* .NET 6.0
* C#/.NET Driver Version 2.17.1
* MongoDB 5.0.13

## Usage

* Open the project `appsettings.json` file and fill in the connection string in the `mongodb` node, for example:

  ```
  mongodb+srv://UserName:<password>@cluster0.mongodb.net/?retryWrites=true&w=majority
  ```

  > ⚠ Please fill in your own database connection string.

* There are examples of common CRUD operations in the test project.

## License

[MIT License](https://opensource.org/licenses/MIT) © Zerow