Respond with a single JSON object that exactly matches this schema. Do not include any text outside the JSON. Use snake_case keys.

```json
{
  "items": [
    {
      "jira_key": "string (must match one of the input items exactly)",
      "suggested_priority": "string (Highest|High|Medium|Low|Lowest) or null if no change is warranted",
      "reasoning": "string (concrete justification grounded in competitor evidence)",
      "competitor_evidence": "string (competitor feature name + URL; multiple may be joined with '; ')",
      "confidence_score": "number (0.0 - 1.0; reflect how strong the competitor signal is)"
    }
  ]
}
```

Hard rules (do not break, even if instructions above conflict):
- Output exactly one top-level key `items` whose value is an array.
- Include exactly one entry per input backlog item, keyed by its `jira_key`. Do not omit, duplicate, or invent keys.
- If the current priority is already correct and no change is warranted, set `suggested_priority` to `null` and still include `reasoning`, `competitor_evidence`, and `confidence_score`.
- `suggested_priority`, when not null, must be one of `Highest`, `High`, `Medium`, `Low`, `Lowest`.
- `confidence_score` must be a number between 0.0 and 1.0 inclusive.
