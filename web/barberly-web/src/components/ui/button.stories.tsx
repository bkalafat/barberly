// Fixed: Removed problematic Storybook type imports
import { Button } from './button';

const meta = {
  title: 'UI/Button',
  component: Button,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
};

export default meta;

export const Default = {
  args: {
    children: 'Default Button',
  },
};

export const Secondary = {
  args: {
    children: 'Secondary Button',
    variant: 'secondary',
  },
};

export const Destructive = {
  args: {
    children: 'Destructive Button',
    variant: 'destructive',
  },
};

export const Outline = {
  args: {
    children: 'Outline Button',
    variant: 'outline',
  },
};

export const Ghost = {
  args: {
    children: 'Ghost Button',
    variant: 'ghost',
  },
};

export const Link = {
  args: {
    children: 'Link Button',
    variant: 'link',
  },
};

export const Large = {
  args: {
    children: 'Large Button',
    size: 'lg',
  },
};

export const Small = {
  args: {
    children: 'Small Button',
    size: 'sm',
  },
};
