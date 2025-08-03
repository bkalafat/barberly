# React UI Prompt’ları (Copilot için)

## RHF + Zod Form Kalıbı

```tsx
const schema = z.object({ ... });
const { register, handleSubmit } = useForm({ resolver: zodResolver(schema) });
```

## TanStack Query Kullanımı

```ts
const { data } = useQuery({ queryKey: [...], queryFn: ... });
```

## Erişilebilirlik & Hata Durumları
- WAI-ARIA, role="alert", odak yönetimi
