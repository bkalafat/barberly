declare module '@storybook/react-vite' {
  // Minimal Storybook config type to satisfy TypeScript until proper types are added
  const config: unknown;
  export default config;
}

declare module '@storybook/react' {
  // Minimal Preview / Story definitions
  export type Preview = unknown;
  export const Preview: Preview;
  const _default: unknown;
  export default _default;
}
