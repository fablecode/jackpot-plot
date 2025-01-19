const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const RtlCssPlugin = require("rtlcss-webpack-plugin");

// Define paths
const rootPath = path.resolve(__dirname);
const distPath = path.join(rootPath, "src/assets");
const entryPath = path.join(distPath, "sass/style.scss");

module.exports = {
  mode: "development",
  stats: "verbose",
  performance: {
    hints: "error",
    maxAssetSize: 10000000,
    maxEntrypointSize: 4000000,
  },
  entry: {
    "css/style": entryPath,
  },
  output: {
    path: distPath,
    filename: "[name].js",
  },
  resolve: {
    extensions: [".scss"],
  },
  plugins: [
    new MiniCssExtractPlugin({
      filename: "[name].rtl.css",
    }),
    new RtlCssPlugin({
      filename: "[name].rtl.css",
    }),
  ],
  module: {
    rules: [
      {
        test: /\.scss$/,
        use: [
          MiniCssExtractPlugin.loader,
          "css-loader",
          {
            loader: "sass-loader",
            options: {
              sourceMap: true,
            },
          },
        ],
      },
    ],
  },
};
