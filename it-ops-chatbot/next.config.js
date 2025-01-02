/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  // Enable SWC compilation cache
  compiler: {
    removeConsole: process.env.NODE_ENV === "production"
  }
}

module.exports = nextConfig 