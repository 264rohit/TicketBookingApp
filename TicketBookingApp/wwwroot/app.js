// When running dev server, open http://localhost:3000
// In production (after building the client), the built React app will be available from this path.
if (location.hostname === 'localhost' && location.port !== '3000') {
  // If backend served on a different port, provide a small notice/link
  document.body.innerHTML = '<div style="font-family: Arial; padding: 20px;"><h2>React Dev Server</h2><p>Run the React dev server: <code>npm --prefix ./client run dev</code></p><p>Then open <a href="http://localhost:3000" target="_blank">http://localhost:3000</a></p></div>';
} else {
  // If built, the static files from client build will be served directly by backend
  // No-op
}
