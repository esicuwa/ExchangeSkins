
[English](#english) | [Русский](#русский) | [中文](#中文)

---

<a name="english"></a>
# ExchangeSkins (English)

> **Note:** The current release is free and has limited functionality. A more capable, paid version is planned for release in the future. For news, updates, and release announcements, follow: https://t.me/software_esic

A C# application for automated CS2 (Counter-Strike 2) skin exchange/crafting using Steam accounts. This tool connects to Steam accounts, filters inventory items by rarity, and automatically crafts items in batches.

## Features

- 🔐 **Steam Authentication**: Secure login using Steam credentials and Steam Guard (.mafile) support
- 🎯 **Rarity Filtering**: Filter items by rarity (Common, Uncommon, Rare, Mythical)
- 🔄 **Batch Crafting**: Automatically crafts items in batches of 10
- 👥 **Multi-Account Support**: Process multiple Steam accounts sequentially

## Requirements

- **.NET 8.0** SDK
- **Steam Account(s)** with CS2 items in inventory
- **Steam Guard Authenticator** (.mafile) for each account
- **Windows** (tested on Windows 10/11)

## Installation

1. **Download the latest release**
   - Go to the [Releases](https://github.com/esicuwa/ExchangeSkins/releases) page
   - Download the archive file

2. **Extract the archive**
   - Extract the downloaded archive
   - The archive contains a folder with the software

3. **Run the executable**
   - Navigate to the extracted folder
   - Double-click `ExchangeSkins.exe` to start the application
   - Make sure you have `config.json` configured in the same directory

## Configuration

Create a `config.json` file in the executable directory with the following structure:

```json
{
  "MaFile_Path": "C:\\Users\\maFiles",
  "Accounts_Path": "C:\\Users\\accounts.txt",
  "Rarity": "Common"
}
```

### Configuration Parameters

- **MaFile_Path**: Path to the folder containing `.mafile` files (Steam Guard authenticator files)
- **Accounts_Path**: Path to a text file containing Steam account credentials
- **Rarity**: Item rarity to filter and craft. The application will craft items of the specified rarity. Valid values:
  - `"Common"` - Common rarity items
  - `"Uncommon"` - Uncommon rarity items
  - `"Rare"` - Rare rarity items
  - `"Mythical"` - Mythical rarity items

### Accounts File Format

Create a text file (e.g., `accounts.txt`) with one account per line in the format:
```
username1:password1
username2:password2
username3:password3
```

### Mafile Files

- Each `.mafile` must be named exactly as the Steam username (case-insensitive)
- Example: If your username is `MySteamAccount`, the file should be named `MySteamAccount.mafile`
- Place all `.mafile` files in the directory specified in `MaFile_Path`

## Usage

1. **Prepare your configuration**
   - Set up `config.json` with correct paths
   - Create accounts file with credentials
   - Place `.mafile` files in the specified directory

2. **Run the application**
   - Run `ExchangeSkins.exe`

3. **Monitor the process**
   - The application will process each account sequentially
   - Wait for each account to complete before moving to the next

## How It Works

1. **Connection**: Connects to Steam using provided credentials
2. **Authentication**: Uses Steam Guard authenticator (.mafile) for 2FA
3. **Inventory Loading**: Loads CS2 inventory items
4. **Filtering**: Filters items by:
   - Specified rarity (items will be crafted from this rarity)
   - Exchangeable items only (excludes stickers, charms, cases, etc.)
   - Items without trade protection only
5. **Crafting**: Crafts items in batches of 10
6. **Confirmation**: Automatically confirms trades via Steam Mobile Authenticator

## Troubleshooting

### Common Issues

1. **"Error: .mafile for account not found"**
   - Ensure the `.mafile` filename matches the username exactly (case-insensitive)
   - Check that the file is in the directory specified in `MaFile_Path`

2. **"Error: invalid Rarity format"**
   - Use one of: `"Common"`, `"Uncommon"`, `"Rare"`, or `"Mythical"` (case-sensitive)

3. **"Error: invalid Account format"**
   - Ensure accounts file uses `username:password` format
   - One account per line

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Disclaimer

This tool is for educational purposes only. Use at your own risk. The authors are not responsible for any account bans, item losses, or other consequences resulting from the use of this software. Automated interaction with Steam may violate Steam's Terms of Service.

---

<a name="русский"></a>
# ExchangeSkins (Русский)

> **Важно:** Эта версия бесплатная и неполная. В будущем выйдет расширенная платная версия с расширенным функционалом — следить за анонсами и датой выхода можно здесь: https://t.me/software_esic

C# приложение для автоматического обмена/крафта скинов CS2 (Counter-Strike 2) с использованием аккаунтов Steam. Этот инструмент подключается к аккаунтам Steam, фильтрует предметы инвентаря по редкости и автоматически крафтит предметы партиями.

## Возможности

- 🔐 **Аутентификация Steam**: Безопасный вход с использованием учетных данных Steam и поддержкой Steam Guard (.mafile)
- 🎯 **Фильтрация по редкости**: Фильтрация предметов по редкости (Common, Uncommon, Rare, Mythical)
- 🔄 **Пакетный крафт**: Автоматически крафтит предметы партиями по 10 штук
- 👥 **Поддержка нескольких аккаунтов**: Обработка нескольких аккаунтов Steam последовательно

## Требования

- **.NET 8.0** SDK
- **Аккаунт(ы) Steam** с предметами CS2 в инвентаре
- **Steam Guard Authenticator** (.mafile) для каждого аккаунта
- **Windows** (протестировано на Windows 10/11)

## Установка

1. **Скачайте последний релиз**
   - Перейдите на страницу [Releases](https://github.com/esicuwa/ExchangeSkins/releases)
   - Скачайте архив

2. **Распакуйте архив**
   - Распакуйте скачанный архив
   - В архиве находится папка с программным обеспечением

3. **Запустите исполняемый файл**
   - Перейдите в распакованную папку
   - Дважды щелкните `ExchangeSkins.exe` для запуска приложения
   - Убедитесь, что файл `config.json` настроен в той же директории

## Конфигурация

Создайте файл `config.json` в директории исполняемого файла со следующей структурой:

```json
{
  "MaFile_Path": "C:\\Users\\maFiles",
  "Accounts_Path": "C:\\Users\\accounts.txt",
  "Rarity": "Common"
}
```

### Параметры конфигурации

- **MaFile_Path**: Путь к папке, содержащей файлы `.mafile` (файлы аутентификатора Steam Guard)
- **Accounts_Path**: Путь к текстовому файлу с учетными данными аккаунтов Steam
- **Rarity**: Редкость предметов для фильтрации и крафта. Приложение будет крафтить предметы из указанной редкости. Допустимые значения:
  - `"Common"` - предметы обычной редкости (Ширпотреб)
  - `"Uncommon"` - предметы необычной редкости (Промышленное)
  - `"Rare"` - предметы редкой редкости (Армейское)
  - `"Mythical"` - предметы мифической редкости (Запрещенное)

### Формат файла аккаунтов

Создайте текстовый файл (например, `accounts.txt`) с одним аккаунтом на строку в формате:
```
username1:password1
username2:password2
username3:password3
```

### Файлы Mafile

- Каждый `.mafile` должен быть назван точно так же, как имя пользователя Steam (без учета регистра)
- Пример: Если ваше имя пользователя `MySteamAccount`, файл должен называться `MySteamAccount.mafile`
- Поместите все файлы `.mafile` в директорию, указанную в `MaFile_Path`

## Использование

1. **Подготовьте конфигурацию**
   - Настройте `config.json` с правильными путями
   - Создайте файл аккаунтов с учетными данными
   - Поместите файлы `.mafile` в указанную директорию

2. **Запустите приложение**
   - Запустите `ExchangeSkins.exe`

3. **Следите за процессом**
   - Приложение будет обрабатывать каждый аккаунт последовательно
   - Дождитесь завершения обработки каждого аккаунта перед переходом к следующему

## Как это работает

1. **Подключение**: Подключается к Steam с использованием предоставленных учетных данных
2. **Аутентификация**: Использует аутентификатор Steam Guard (.mafile) для 2FA
3. **Загрузка инвентаря**: Загружает предметы инвентаря CS2
4. **Фильтрация**: Фильтрует предметы по:
   - Указанной редкости (из этой редкости будут крафтиться предметы)
   - Только обмениваемым предметам (исключает стикеры, чары, кейсы и т.д.)
   - Только предметы без трейдпротекта
5. **Крафт**: Крафтит предметы партиями по 10 штук
6. **Подтверждение**: Автоматически подтверждает обмены через Steam Mobile Authenticator

## Решение проблем

### Частые проблемы

1. **"Error: .mafile for account not found"**
   - Убедитесь, что имя файла `.mafile` точно совпадает с именем пользователя (без учета регистра)
   - Проверьте, что файл находится в директории, указанной в `MaFile_Path`

2. **"Error: invalid Rarity format"**
   - Используйте одно из: `"Common"`, `"Uncommon"`, `"Rare"`, или `"Mythical"` (с учетом регистра)

3. **"Error: invalid Account format"**
   - Убедитесь, что файл аккаунтов использует формат `username:password`
   - Один аккаунт на строку

## Лицензия

Этот проект лицензирован под лицензией MIT - см. файл [LICENSE.txt](LICENSE.txt) для подробностей.

## Отказ от ответственности

Этот инструмент предназначен только для образовательных целей. Используйте на свой риск. Авторы не несут ответственности за блокировки аккаунтов, потерю предметов или другие последствия, возникшие в результате использования этого программного обеспечения. Автоматизированное взаимодействие со Steam может нарушать Условия использования Steam.

---

<a name="中文"></a>
# ExchangeSkins (中文)

> **说明：** 当前版本免费且功能有限。功能更全的付费版本将在未来推出，发布动态与更新信息请关注：https://t.me/software_esic

用于通过 Steam 账号自动交换/合成 CS2（反恐精英 2）皮肤的 C# 应用程序。本工具连接 Steam 账号，按稀有度筛选库存物品，并自动批量合成物品。

## 功能特点

- 🔐 **Steam 认证**：使用 Steam 凭据安全登录，支持 Steam Guard (.mafile)
- 🎯 **稀有度筛选**：按稀有度筛选物品（普通、非凡、稀有、神话）
- 🔄 **批量合成**：自动以每批 10 个物品进行合成
- 👥 **多账号支持**：依次处理多个 Steam 账号

## 系统要求

- **.NET 8.0** SDK
- 库存中有 CS2 物品的 **Steam 账号**
- 每个账号的 **Steam Guard 验证器** (.mafile)
- **Windows**（已在 Windows 10/11 上测试）

## 安装

1. **下载最新版本**
   - 前往 [Releases](https://github.com/esicuwa/ExchangeSkins/releases) 页面
   - 下载压缩包

2. **解压文件**
   - 解压下载的压缩包
   - 压缩包内包含软件所在文件夹

3. **运行程序**
   - 进入解压后的文件夹
   - 双击 `ExchangeSkins.exe` 启动程序
   - 确保同目录下已正确配置 `config.json`

## 配置

在程序所在目录创建 `config.json` 文件，结构如下：

```json
{
  "MaFile_Path": "C:\\Users\\maFiles",
  "Accounts_Path": "C:\\Users\\accounts.txt",
  "Rarity": "Common"
}
```

### 配置参数

- **MaFile_Path**：存放 `.mafile` 文件（Steam Guard 验证器文件）的文件夹路径
- **Accounts_Path**：包含 Steam 账号凭据的文本文件路径
- **Rarity**：用于筛选和合成的物品稀有度。程序将合成指定稀有度的物品。有效值：
  - `"Common"` - 普通
  - `"Uncommon"` - 非凡
  - `"Rare"` - 稀有
  - `"Mythical"` - 神话

### 账号文件格式

创建文本文件（如 `accounts.txt`），每行一个账号，格式为：
```
username1:password1
username2:password2
username3:password3
```

### Mafile 文件

- 每个 `.mafile` 的文件名必须与 Steam 用户名完全一致（不区分大小写）
- 示例：用户名为 `MySteamAccount` 时，文件应命名为 `MySteamAccount.mafile`
- 将所有 `.mafile` 文件放在 `MaFile_Path` 指定的目录中

## 使用说明

1. **准备配置**
   - 在 `config.json` 中设置正确路径
   - 创建包含凭据的账号文件
   - 将 `.mafile` 文件放入指定目录

2. **运行程序**
   - 运行 `ExchangeSkins.exe`

3. **监控过程**
   - 程序将依次处理每个账号
   - 等待当前账号处理完成后再处理下一个

## 工作原理

1. **连接**：使用提供的凭据连接 Steam
2. **认证**：使用 Steam Guard 验证器 (.mafile) 进行两步验证
3. **加载库存**：加载 CS2 库存物品
4. **筛选**：按以下条件筛选物品：
   - 指定稀有度（将使用该稀有度的物品进行合成）
   - 仅可交换物品（排除贴纸、挂件、箱子等）
   - 仅无交易保护期的物品
5. **合成**：每批合成 10 个物品
6. **确认**：通过 Steam 移动验证器自动确认交易

## 常见问题

### 常见错误

1. **"Error: .mafile for account not found"（未找到账号的 .mafile）**
   - 确保 `.mafile` 文件名与用户名完全一致（不区分大小写）
   - 确认文件位于 `MaFile_Path` 指定的目录中

2. **"Error: invalid Rarity format"（Rarity 格式无效）**
   - 使用以下之一：`"Common"`、`"Uncommon"`、`"Rare"` 或 `"Mythical"`（区分大小写）

3. **"Error: invalid Account format"（账号格式无效）**
   - 确保账号文件使用 `username:password` 格式
   - 每行一个账号

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE.txt](LICENSE.txt) 文件。

## 免责声明

本工具仅供学习使用。使用风险自负。作者不对因使用本软件导致的账号封禁、物品损失或其他后果负责。对 Steam 的自动化操作可能违反 Steam 服务条款。
