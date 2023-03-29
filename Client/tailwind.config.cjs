/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    colors: {
      "background-dark1": "var(--background-dark1)",
      "background-dark2": "var(--background-dark2)",
      "background-dark3": "var(--background-dark3)",
      "color-primary": 'var(--color-primary)',
    },
    extend: {},
  },
  plugins: [],
}
