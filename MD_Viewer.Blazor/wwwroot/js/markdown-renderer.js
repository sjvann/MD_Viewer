import { marked } from 'marked';
import hljs from 'highlight.js';
import * as katex from 'katex';
/**
 * 渲染 Markdown 內容為 HTML
 * @param content Markdown 內容
 * @returns 渲染後的 HTML 字串
 */
export function renderMarkdown(content) {
    if (!content) {
        return '';
    }
    // 設定 marked 選項（marked v11 使用 renderer）
    const renderer = new marked.Renderer();
    const originalCode = renderer.code.bind(renderer);
    renderer.code = (code, language, escaped) => {
        let highlightedCode = code;
        if (language && hljs.getLanguage(language)) {
            try {
                highlightedCode = hljs.highlight(code, { language }).value;
            }
            catch (err) {
                // 如果語言不支援，使用自動檢測
                highlightedCode = hljs.highlightAuto(code).value;
            }
        }
        else {
            highlightedCode = hljs.highlightAuto(code).value;
        }
        return originalCode(highlightedCode, language, escaped);
    };
    marked.use({ renderer });
    // 解析 Markdown
    let html = marked.parse(content);
    // 處理數學公式（區塊公式：$$...$$）
    html = html.replace(/\$\$([\s\S]*?)\$\$/g, (match, formula) => {
        try {
            return katex.renderToString(formula.trim(), { displayMode: true });
        }
        catch (err) {
            // 如果公式錯誤，保留原樣
            return match;
        }
    });
    // 處理數學公式（行內公式：$...$）
    // 注意：需要避免匹配 $$...$$ 的情況
    html = html.replace(/(?<!\$)\$([^\$\n]+?)\$(?!\$)/g, (match, formula) => {
        try {
            return katex.renderToString(formula.trim(), { displayMode: false });
        }
        catch (err) {
            // 如果公式錯誤，保留原樣
            return match;
        }
    });
    return html;
}
//# sourceMappingURL=markdown-renderer.js.map