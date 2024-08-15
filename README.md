<div align="center">
  
  # HamsterFramework
  
</div>

<p align="center">
  <img src="https://github.com/lipeilin2006/HamsterFramework/blob/main/Hamster.png" style="height:200px;width:auto;" alt="icon"/>
</p>

<p align="center">
  <a href="https://github.com/lipeilin2006/HamsterFramework/actions/workflows/Build.yml">
    <img src="https://github.com/lipeilin2006/HamsterFramework/actions/workflows/Build.yml/badge.svg" alt="build">
  </a>
  <a href="https://raw.githubusercontent.com/lipeilin2006/HamsterFramework/main/LICENSE">
    <img src="https://img.shields.io/github/license/lipeilin2006/HamsterFramework" alt="license">
  </a>
  <a href="https://github.com/lipeilin2006/HamsterFramework/releases">
    <img src="https://img.shields.io/github/v/release/lipeilin2006/HamsterFramework?color=blueviolet&include_prereleases" alt="release">
  </a>
</p>

<br />

这是一个非常简单的C#后端框架，跨平台，支持热更新。

# 运行指南
将全部编译后，将生成的Hamster.Hotfix.dll复制到Hamster.txt所在目录下，再运行Hamster.exe。

# 项目说明
Hamster为主程序项目。
Hamster.Core为HttpServer核心项目。
Hamster.Utils不被服务端项目依赖，可以被Hamster.Hotfix引用。
Hamster.Hotfix为热更新项目，只存放与处理请求相关的逻辑，不能引用Hamster和Hamster.Core。