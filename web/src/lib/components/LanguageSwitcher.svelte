<script lang="ts">
  import { locale, setLocale, localeLabels, localeNativeNames, type Locale } from '$lib/i18n';

  const locales: Locale[] = ['en', 'ru', 'kk'];
</script>

<div class="lang-switcher" role="group" aria-label="Language">
  {#each locales as l}
    <button
      class="lang-btn"
      class:active={$locale === l}
      onclick={() => setLocale(l)}
      title={localeNativeNames[l]}
      aria-pressed={$locale === l}
    >
      {localeLabels[l]}
    </button>
  {/each}
</div>

<style>
  .lang-switcher {
    display: flex;
    align-items: center;
    gap: 1px;
    background: var(--bg-sunk);
    border: var(--hairline);
    border-radius: var(--radius);
    padding: 2px;
    flex-shrink: 0;
  }

  .lang-btn {
    background: transparent;
    border: none;
    border-radius: calc(var(--radius) - 2px);
    color: var(--ink-faint);
    cursor: pointer;
    font-family: var(--font-mono);
    font-size: 0.7rem;
    font-weight: 500;
    letter-spacing: 0.06em;
    line-height: 1;
    padding: 4px 7px;
    transition: background 120ms ease, color 120ms ease;
    white-space: nowrap;
  }

  .lang-btn:hover:not(.active) {
    background: var(--bg-elev);
    color: var(--ink-soft);
  }

  .lang-btn.active {
    background: var(--accent);
    color: #fff;
  }
</style>
