import { marked } from 'marked';
import hljs from 'highlight.js';
import * as katex from 'katex';

/**
 * 渲染 Markdown 內容為 HTML
 * @param content Markdown 內容
 * @returns 渲染後的 HTML 字串
 */
export function renderMarkdown(content: string): string {
	if (!content) {
		return '';
	}

	// 設定 marked 選項（marked v11 使用 renderer）
	const renderer = new marked.Renderer();
	const originalCode = renderer.code.bind(renderer);
	renderer.code = (code: string, language?: string, escaped?: boolean) => {
		let highlightedCode = code;
		if (language && hljs.getLanguage(language)) {
			try {
				highlightedCode = hljs.highlight(code, { language }).value;
			} catch (err) {
				// 如果語言不支援，使用自動檢測
				highlightedCode = hljs.highlightAuto(code).value;
			}
		} else {
			highlightedCode = hljs.highlightAuto(code).value;
		}
		return originalCode(highlightedCode, language, escaped ?? false);
	};

	marked.use({ renderer });

	// 解析 Markdown
	let html = marked.parse(content) as string;

	// 處理數學公式（區塊公式：$$...$$）
	html = html.replace(/\$\$([\s\S]*?)\$\$/g, (match: string, formula: string) => {
		try {
			return katex.renderToString(formula.trim(), { displayMode: true });
		} catch (err) {
			// 如果公式錯誤，保留原樣
			return match;
		}
	});

	// 處理數學公式（行內公式：$...$）
	// 注意：需要避免匹配 $$...$$ 的情況
	// 此正則表達式使用 lookbehind assertion (?<!\$)，需要支援 ES2018 的瀏覽器
	// 現代瀏覽器（Chrome 62+, Firefox 78+, Safari 16.4+, Edge 79+）都支援
	html = html.replace(/(?<!\$)\$([^\$\n]+?)\$(?!\$)/g, (match: string, formula: string) => {
		try {
			return katex.renderToString(formula.trim(), { displayMode: false });
		} catch (err) {
			// 如果公式錯誤，保留原樣
			return match;
		}
	});

	return html;
}

