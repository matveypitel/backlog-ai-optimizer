Respond with a single JSON object that exactly matches this schema. Do not include any text outside the JSON. Use snake_case keys.

```json
{
  "features": [
    {
      "name": "string (3-8 words, concrete user-facing capability, no marketing fluff)",
      "description": "string (1-3 sentences explaining what the feature does for the user)",
      "category": "string or null (e.g. Analytics, Collaboration, Integrations, Automation, Reporting)",
      "key_benefits": ["string (specific outcome the user gains, 1 short sentence each, 2-5 items)"],
      "use_cases": ["string (concrete real-world scenario where the feature applies, 1 short sentence each, 2-5 items)"],
      "differentiators": "string (what makes this feature notable or hard to replicate; empty string if nothing distinctive)",
      "target_audience": "string or null (specific user persona or role this feature is built for)"
    }
  ]
}
```

Hard rules (do not break, even if instructions above conflict):
- Output exactly one top-level key `features` whose value is an array. Use `[]` if the page has no product features.
- Every array item must include every field listed above. Use empty arrays `[]` for `key_benefits` / `use_cases` only if the page truly has no signal; otherwise infer from context.
- `name` must be unique within the response and non-overlapping with other features.
- Never invent capabilities not supported by the page content. If unsure, omit the feature.
