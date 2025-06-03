const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'http://localhost:5002',
      changeOrigin: true,
      secure: false,
      onError: (err, req, res) => {
        console.error('Proxy Error:', err);
      }
    })
  );
};
