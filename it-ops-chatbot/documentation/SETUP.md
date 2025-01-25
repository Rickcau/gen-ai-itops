# Project Setup Guide

## 1. Create a New Next.js Project

```bash
npx create-next-app@latest my-frontend
```

During the setup, select the following options:
- Would you like to use TypeScript? › Yes
- Would you like to use ESLint? › Yes
- Would you like to use Tailwind CSS? › Yes
- Would you like to use `src/` directory? › No
- Would you like to use App Router? › Yes
- Would you like to customize the default import alias? › Yes (use @/*)

## 2. Install Dependencies

Navigate to your project directory:
```bash
cd myfrontend
```

Install required dependencies:
```bash
# Install UI and styling dependencies
npm install @radix-ui/react-dialog
npm install @radix-ui/react-slot
npm install @radix-ui/react-label
npm install class-variance-authority
npm install clsx
npm install tailwind-merge
npm install lucide-react

# Install form handling
npm install react-hook-form
npm install @hookform/resolvers
npm install zod

# Install additional utilities
npm install date-fns
```

## 3. Verify Installation

Check your package.json to ensure all dependencies are listed correctly:
```bash
npm list
```

Your package.json dependencies should include:
```json
{
  "dependencies": {
    "@hookform/resolvers": "^3.x.x",
    "@radix-ui/react-dialog": "^1.x.x",
    "@radix-ui/react-label": "^2.x.x",
    "@radix-ui/react-slot": "^1.x.x",
    "class-variance-authority": "^0.7.x",
    "clsx": "^2.x.x",
    "date-fns": "^2.x.x",
    "lucide-react": "^0.x.x",
    "next": "14.x.x",
    "react": "^18.x.x",
    "react-dom": "^18.x.x",
    "react-hook-form": "^7.x.x",
    "tailwind-merge": "^2.x.x",
    "zod": "^3.x.x"
  }
}
```

## 4. Configure Tailwind CSS

Ensure your tailwind.config.ts includes necessary configurations:
```typescript
import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      // Add any custom theme extensions here
    },
  },
  plugins: [],
};

export default config;
```

## 5. Test the Setup

1. Start the development server:
```bash
npm run dev
```

2. Open your browser and navigate to:
```
http://localhost:3000
```

3. Verify that:
- The Next.js application loads without errors
- Tailwind CSS styles are working
- Components can be imported and rendered

## 6. Project Structure Verification

Ensure your project structure looks like this:
```
my-project/
├── src/
│   ├── app/
│   │   ├── layout.tsx
│   │   └── page.tsx
│   ├── components/
│   │   └── ui/
│   └── lib/
├── public/
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

## 7. Common Issues and Troubleshooting

If you encounter any issues:

1. Clear your Next.js cache:
```bash
rm -rf .next
```

2. Delete node_modules and reinstall:
```bash
rm -rf node_modules
rm package-lock.json
npm install
```

3. Verify TypeScript configuration:
```bash
npx tsc --noEmit
```

4. Check for conflicting dependencies:
```bash
npm dedupe
```

## 8. Additional Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [React Hook Form Documentation](https://react-hook-form.com/)
- [Radix UI Documentation](https://www.radix-ui.com/docs/primitives/overview/introduction) 