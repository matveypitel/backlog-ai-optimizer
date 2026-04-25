Respond with a single JSON object that exactly matches this schema. Do not include any text outside the JSON. Use snake_case keys.

```json
{
  "suggestions": [
    {
      "title": "string (5-10 words, concrete and actionable, e.g. 'Real-time collaborative editing for dashboards')",
      "description": "string (2-4 sentences describing the feature concretely — what it does, how it works at a high level)",
      "issue_type": "Story | Task | Epic",
      "suggested_priority": "Highest | High | Medium | Low | Lowest",
      "tags": ["string (short topical labels, 2-5 items)"],
      "reasoning": "string (why this fills a gap given the current backlog and competitor signal)",
      "competitor_evidence": "string (competitor feature name + URL, or multiple separated by '; ')",
      "business_value": "string (1-2 sentences — concrete value to users or business, not marketing)",
      "estimated_impact": "string (must start with one of High|Medium|Low followed by ' — ' and a short justification)",
      "user_stories": [
        "string (format: 'As a <role>, I want <capability>, so that <outcome>'; minimum 2 items, prefer 3)"
      ],
      "acceptance_criteria": [
        "string (format: 'Given <context>, when <action>, then <observable outcome>'; minimum 3 items)"
      ]
    }
  ]
}
```

Hard rules (do not break, even if instructions above conflict):
- Output exactly one top-level key `suggestions` whose value is an array.
- Every suggestion must include every field above. No nulls, no empty strings except where the schema explicitly allows.
- `user_stories` must contain at least 2 items, each in the "As a … I want … so that …" form.
- `acceptance_criteria` must contain at least 3 items, each in the "Given … when … then …" form.
- `estimated_impact` must begin with `High`, `Medium`, or `Low` followed by ` — ` and a justification (≤ 25 words).
- `issue_type` must be one of `Story`, `Task`, `Epic`. `suggested_priority` must be one of `Highest`, `High`, `Medium`, `Low`, `Lowest`.
- Suggestions must be distinct from each other and not duplicate items already present in the backlog.
