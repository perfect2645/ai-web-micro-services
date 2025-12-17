# 创建项目
yarn create vite ui-user-app --template react-ts

# 安装依赖

## 基础依赖
cd ui-user-app
yarn add sass
yarn add axios
## tailwindcss
yarn add tailwindcss @tailwindcss/vite
yarn add -D tailwindcss postcss autoprefixer
> 安装Tailwind依赖（postcss/autoprefixer是Tailwind的必要依赖）
## 图片上传（拖拽/点击）
yarn add react-dropzone
yarn add -D @types/react-dropzone

## 全局提示组件
yarn add react-toastify


# 规划项目目录结构
src/
├── components/          # 业务组件
│   ├── ImageUploader/   # 图片上传组件（拖拽+预览）
│   │   ├── index.tsx
│   │   └── styles.module.scss
│   └── RecognitionResult/ # 识别结果展示组件
│       ├── index.tsx
│       └── styles.module.scss
├── services/            # API服务层
│   ├── api.ts           # axios全局配置
│   └── recognition.ts   # 图片识别接口封装
├── styles/              # 全局样式
│   ├── global.scss      # 全局样式/重置样式
│   └── variables.scss   # SCSS变量（主题色/圆角等）
├── types/               # TypeScript类型定义
│   └── index.ts
├── App.tsx              # 根组件（整合所有功能）
├── main.tsx             # 入口文件
└── vite-env.d.ts        # Vite类型声明


### Card 组件
yarn global add shadcn-ui
npx shadcn@latest add https://21st.dev/r/Codehagen/display-cards