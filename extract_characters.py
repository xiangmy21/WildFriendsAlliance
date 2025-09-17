#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import re
import json
from collections import defaultdict
import argparse

def should_process_file(file_path):
    """判断是否应该处理该文件"""
    # 检查扩展名
    valid_extensions = {'.cs', '.json', '.txt', '.md', '.prefab', '.unity', '.asset'}
    _, ext = os.path.splitext(file_path.lower())

    if ext not in valid_extensions:
        return False

    # 排除不需要的文件
    exclude_patterns = [
        'Library/', 'Temp/', 'Build/', '.git/',
        '.meta', '.dll', '.exe', 'packages-lock.json',
        'DOTween', 'TextMesh Pro/Resources/', 'TextMesh Pro/Sprites/'
    ]

    for pattern in exclude_patterns:
        if pattern in file_path:
            return False

    return True

def extract_characters_from_file(file_path):
    """从单个文件中提取字符"""
    characters = set()
    char_frequency = defaultdict(int)

    try:
        # 尝试不同的编码方式读取文件
        content = None
        for encoding in ['utf-8', 'gbk', 'latin-1']:
            try:
                with open(file_path, 'r', encoding=encoding) as f:
                    content = f.read()
                break
            except UnicodeDecodeError:
                continue

        if content is None:
            print(f"警告: 无法读取文件 {file_path}")
            return characters, char_frequency

        # 提取字符
        for char in content:
            # 跳过控制字符（除了常用空白字符）
            if ord(char) < 32 and char not in ['\n', '\r', '\t']:
                continue

            characters.add(char)
            char_frequency[char] += 1

        print(f"处理文件: {os.path.basename(file_path)} ({len(characters)} 个唯一字符)")

    except Exception as e:
        print(f"处理文件 {file_path} 时出错: {e}")

    return characters, char_frequency

def categorize_characters(characters):
    """将字符分类"""
    categories = {
        'ascii_letters': set(),
        'ascii_digits': set(),
        'ascii_symbols': set(),
        'chinese': set(),
        'chinese_symbols': set(),
        'whitespace': set(),
        'other': set()
    }

    for char in characters:
        code = ord(char)

        if 'A' <= char <= 'Z' or 'a' <= char <= 'z':
            categories['ascii_letters'].add(char)
        elif '0' <= char <= '9':
            categories['ascii_digits'].add(char)
        elif 32 <= code <= 126:  # 可打印ASCII
            categories['ascii_symbols'].add(char)
        elif 0x4E00 <= code <= 0x9FFF:  # CJK统一汉字
            categories['chinese'].add(char)
        elif 0x3000 <= code <= 0x303F:  # CJK符号和标点
            categories['chinese_symbols'].add(char)
        elif char in [' ', '\t', '\n', '\r']:
            categories['whitespace'].add(char)
        else:
            categories['other'].add(char)

    return categories

def generate_character_file(all_characters, char_frequency, output_file):
    """生成字符文件"""

    # 分类字符
    categories = categorize_characters(all_characters)

    # 准备输出内容
    lines = []

    # 文件头
    lines.append("# 从WildFriendsAlliance项目中提取的所有字符")
    lines.append(f"# 生成时间: {__import__('datetime').datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    lines.append(f"# 总唯一字符数: {len(all_characters)}")
    lines.append(f"# 总出现次数: {sum(char_frequency.values())}")
    lines.append("")

    # ASCII字母
    if categories['ascii_letters']:
        lines.append("# === ASCII字母 ===")
        sorted_letters = sorted(categories['ascii_letters'])
        lines.append(''.join(sorted_letters))
        lines.append("")

    # ASCII数字
    if categories['ascii_digits']:
        lines.append("# === ASCII数字 ===")
        sorted_digits = sorted(categories['ascii_digits'])
        lines.append(''.join(sorted_digits))
        lines.append("")

    # ASCII符号
    if categories['ascii_symbols']:
        lines.append("# === ASCII符号 ===")
        sorted_symbols = sorted(categories['ascii_symbols'])
        lines.append(''.join(sorted_symbols))
        lines.append("")

    # 中文字符
    if categories['chinese']:
        lines.append("# === 中文字符 ===")
        sorted_chinese = sorted(categories['chinese'])
        # 每行80个字符，便于阅读
        chinese_text = ''.join(sorted_chinese)
        for i in range(0, len(chinese_text), 80):
            lines.append(chinese_text[i:i+80])
        lines.append("")

    # 中文符号
    if categories['chinese_symbols']:
        lines.append("# === 中文符号 ===")
        sorted_chinese_symbols = sorted(categories['chinese_symbols'])
        lines.append(''.join(sorted_chinese_symbols))
        lines.append("")

    # 空白字符
    if categories['whitespace']:
        lines.append("# === 空白字符 ===")
        whitespace_display = []
        for char in sorted(categories['whitespace']):
            if char == ' ':
                whitespace_display.append('空格')
            elif char == '\t':
                whitespace_display.append('制表符')
            elif char == '\n':
                whitespace_display.append('换行符')
            elif char == '\r':
                whitespace_display.append('回车符')
            else:
                whitespace_display.append(f'U+{ord(char):04X}')
        lines.append(' '.join(whitespace_display))
        lines.append("")

    # 其他字符
    if categories['other']:
        lines.append("# === 其他字符 ===")
        sorted_other = sorted(categories['other'])
        lines.append(''.join(sorted_other))
        lines.append("")

    # 使用频率最高的字符
    lines.append("# === 使用频率最高的50个字符 ===")
    sorted_by_freq = sorted(char_frequency.items(), key=lambda x: x[1], reverse=True)
    for i, (char, freq) in enumerate(sorted_by_freq[:50]):
        if char == '\n':
            char_display = '\\n'
        elif char == '\r':
            char_display = '\\r'
        elif char == '\t':
            char_display = '\\t'
        elif char == ' ':
            char_display = '空格'
        else:
            char_display = char
        lines.append(f"# {char_display} (出现{freq}次)")

    lines.append("")
    lines.append("# === 所有字符（用于TextMesh Pro） ===")

    # 为TextMesh Pro生成字符串（排除控制字符）
    tmp_chars = []
    for char in sorted(all_characters):
        if char not in ['\n', '\r', '\t'] and ord(char) >= 32:
            tmp_chars.append(char)

    # 每行100个字符
    tmp_text = ''.join(tmp_chars)
    for i in range(0, len(tmp_text), 100):
        lines.append(tmp_text[i:i+100])

    # 写入文件
    try:
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines))
        print(f"\n字符文件已生成: {output_file}")
        print(f"总共包含 {len(all_characters)} 个唯一字符")
        return True
    except Exception as e:
        print(f"写入文件时出错: {e}")
        return False

def main():
    parser = argparse.ArgumentParser(description='从项目文件中提取所有字符')
    parser.add_argument('--output', '-o', default='characters.txt', help='输出文件名')
    parser.add_argument('--chinese-only', action='store_true', help='仅提取中文字符')

    args = parser.parse_args()

    # 搜索目录
    search_dirs = ['.', 'questions', 'Assets']

    all_characters = set()
    char_frequency = defaultdict(int)

    print("开始提取项目中的所有字符...")

    file_count = 0
    for search_dir in search_dirs:
        if not os.path.exists(search_dir):
            continue

        for root, dirs, files in os.walk(search_dir):
            # 排除不需要的目录
            dirs[:] = [d for d in dirs if d not in ['Library', 'Temp', 'Build', '.git']]

            for file in files:
                file_path = os.path.join(root, file)

                if should_process_file(file_path):
                    chars, freq = extract_characters_from_file(file_path)
                    all_characters.update(chars)

                    for char, count in freq.items():
                        char_frequency[char] += count

                    file_count += 1

    print(f"\n处理了 {file_count} 个文件")

    # 如果只要中文字符，过滤结果
    if args.chinese_only:
        chinese_chars = set()
        chinese_freq = defaultdict(int)

        for char in all_characters:
            code = ord(char)
            if 0x4E00 <= code <= 0x9FFF or 0x3000 <= code <= 0x303F:
                chinese_chars.add(char)
                chinese_freq[char] = char_frequency[char]

        all_characters = chinese_chars
        char_frequency = chinese_freq
        args.output = 'chinese_characters.txt'
        print(f"过滤后仅保留中文字符: {len(all_characters)} 个")

    # 生成字符文件
    success = generate_character_file(all_characters, char_frequency, args.output)

    if success:
        print(f"\n✅ 字符提取完成！")
        print(f"📁 输出文件: {args.output}")
        print(f"🔤 唯一字符数: {len(all_characters)}")
        print(f"📊 总出现次数: {sum(char_frequency.values())}")

        # 显示各类字符统计
        categories = categorize_characters(all_characters)
        print(f"\n📈 字符分类统计:")
        print(f"  ASCII字母: {len(categories['ascii_letters'])}")
        print(f"  ASCII数字: {len(categories['ascii_digits'])}")
        print(f"  ASCII符号: {len(categories['ascii_symbols'])}")
        print(f"  中文字符: {len(categories['chinese'])}")
        print(f"  中文符号: {len(categories['chinese_symbols'])}")
        print(f"  空白字符: {len(categories['whitespace'])}")
        print(f"  其他字符: {len(categories['other'])}")

if __name__ == '__main__':
    main()