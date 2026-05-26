import { writable, derived } from 'svelte/store';
import { en, type Translations } from './en';
import { ru } from './ru';
import { kk } from './kk';

export type Locale = 'en' | 'ru' | 'kk';
export type { Translations };

const STORAGE_KEY = 'app-locale';
const VALID_LOCALES: Locale[] = ['en', 'ru', 'kk'];

function loadSavedLocale(): Locale {
  if (typeof localStorage === 'undefined') return 'ru';
  const saved = localStorage.getItem(STORAGE_KEY) as Locale;
  return VALID_LOCALES.includes(saved) ? saved : 'ru';
}

export const locale = writable<Locale>('ru');

// Initialize from localStorage after module loads (client-side only)
if (typeof window !== 'undefined') {
  locale.set(loadSavedLocale());
}

locale.subscribe((val) => {
  if (typeof localStorage !== 'undefined') {
    localStorage.setItem(STORAGE_KEY, val);
  }
});

const dictionaries: Record<Locale, Translations> = { en, ru, kk };

/** Reactive translations store — use as `$T` in components */
export const T = derived(locale, ($l) => dictionaries[$l]);

export const localeLabels: Record<Locale, string> = {
  en: 'EN',
  ru: 'RU',
  kk: 'KZ',
};

export const localeNativeNames: Record<Locale, string> = {
  en: 'English',
  ru: 'Русский',
  kk: 'Қазақша',
};

export function setLocale(l: Locale): void {
  locale.set(l);
}
