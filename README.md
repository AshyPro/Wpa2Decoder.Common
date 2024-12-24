# Ashy.Wpa2Decoder.Library

A .NET library for parsing `.pcap` files, analyzing EAPOL (Extensible Authentication Protocol over LAN) handshake parameters, computing WPA2 cryptographic values, and generating a password dictionary for dictionary-based attacks.

![Ashy.pro](icon.jpg)

## Author Information 
- **Author**: Oleksii Shyrokov
- **Email**: [oleksii@ashy.pro](mailto:oleksii@ashy.pro)
- **Web**: [https://ashy.pro/oleksii](ashy.pro)

## Project Overview

**Ashy.Wpa2Decoder.Library** is a C# class library designed for analyzing WPA2-secured Wi-Fi networks by parsing `.pcap` (Packet Capture) files. It focuses on extracting and processing EAPOL handshake data, enabling cryptographic analysis, and performing password testing through dictionary-based attacks. The library calculates crucial cryptographic elements used in WPA2 handshakes, including the Pairwise Transient Key (PTK) and Message Integrity Code (MIC), without the need for large dictionary files. Instead, it generates a dynamic password dictionary tailored to the captured handshake, minimizing storage overhead.

This library supports .NET 9 and higher.

## Features

- **PCAP File Parsing**  
  Reads and parses `.pcap` files containing wireless network traffic. Extracts a comprehensive **PcapSummary** containing Wi-Fi network details and all relevant **EAPOL** handshake data, such as **BSSID**, **Client MAC**, **Cipher suite**, **ANONCE**, **SNONCE**, and MIC.

- **EAPOL Handshake Detection**  
  Automatically identifies and extracts **handshake** messages from EAPOL packets in the capture. It collects essential handshake parameters including the ANONCE (Authenticator Nonce), SNONCE (Supplicant Nonce), BSSID (AP MAC address), and more.

- **Wi-Fi Network Summary**  
  Generates a serializable summary to be saved to a **JSON** file listing all Wi-Fi networks detected within the `.pcap` file, including details on encryption types, ESSID, and BSSID.

- **WPA2 Cryptographic Functions**  
  Provides methods to compute WPA2-related cryptographic elements:
  - **Pairwise Master Key (PMK)** generation from a passphrase and handshake data.
  - **Pairwise Transient Key (PTK)** generation from a PMK and handshake data.
  - **Message Integrity Code (MIC)** generation and comparison to validate the integrity of handshake messages.
  - **Password Validation**: Tests a password by comparing the generated MIC with the one in the captured handshake.

- **Password Dictionary Generation**  
  Supports the dynamic generation of custom password dictionaries for dictionary-based attacks, with the following configurable options:
  - **Password Length**: Minimum and maximum length constraints.
  - **Custom Word Lists**: Use predefined word lists (e.g., names or keywords related to the Wi-Fi network).
  - **Combinatorial Rules**: Combine two words, apply custom word separators, or add padding before/after words.
  - **Word Modification**: Apply transformations to words, such as converting "four" to "4", "ex" to "x", or applying common character substitutions (e.g., "i" → "1").
  - **Case Variations**: Generate all combinations of upper and lower case letters, or alternatively, only the first letter of each word.

- **No Need for Large Dictionary Files**  
  The dictionary generation process does not require the storage of large wordlists. All password candidates are tested on the fly, reducing storage overhead and disk usage.

## Comparison to Aircrack-ng

While Aircrack-ng is a well-established and popular suite for auditing wireless networks, including capabilities for capturing handshakes and performing dictionary attacks, Ashy.Wpa2Decoder.Library is designed specifically for integration into .NET applications. Unlike Aircrack-ng, which is primarily a command-line tool, Ashy.Wpa2Decoder.Library offers a C# API that can be directly used within .NET applications, making it easier to integrate into existing projects.

In addition, Ashy.Wpa2Decoder.Library offers a unique dynamic password dictionary generation feature that eliminates the need for large pre-built dictionary files. This feature allows users to generate and test password candidates on the fly, saving disk space and making it more flexible for testing a wide range of potential passwords based on custom rules, patterns, and variations.

While both tools perform similar tasks, Ashy.Wpa2Decoder.Library is aimed at .NET developers who want to automate or integrate WPA2 analysis into their own software, while Aircrack-ng remains a powerful and versatile standalone tool for manual and scripted use.

## Installation

You can install the library via [NuGet](https://nuget.org/packages/Ashy.Wpa2Decoder.Library).

```bash
dotnet add package Ashy.Wpa2Decoder.Library
```

## Usage

This library can be used in both console applications and other C# projects. A sample console application is available, which demonstrates how to analyze .pcap files and perform dictionary-based attacks without relying on pre-generated dictionary files.

Code example:
```
scanResult = PcapScanner.ScanFile(parameters);
...
result = PasswordDictionaryGenerator.DictionaryAttack(parameters);
```

For an example, visit the related console app repository:
[Console app](https://github.com/AshyPro/wpa2decoder): `https://github.com/AshyPro/wpa2decoder`

## Disclaimer

AAshy.Wpa2Decoder.Library is intended for educational purposes and authorized security testing only. It is designed to demonstrate the importance of selecting strong, secure passwords for Wi-Fi networks. The use of this library for unauthorized activities, such as accessing networks without permission, is illegal and unethical.

By using this library, you agree to take full responsibility for your actions and ensure they comply with all applicable laws and regulations. The author and contributors are not responsible for any illegal activities or damages resulting from the use or misuse of this library. Always obtain proper authorization before performing any security assessments or penetration testing.

This disclaimer emphasizes the educational purpose and highlights the importance of strong passwords in a professional and concise manner.

## License

This project is licensed under the MIT License