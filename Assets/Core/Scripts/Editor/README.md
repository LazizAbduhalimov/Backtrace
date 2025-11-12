# Unity Menu Bar & Title Bar Hider

## Описание
Простая система для скрытия Menu Bar (File, Edit, Assets, Tools и т.д.) и Title Bar в Unity Editor для увеличения рабочей области.

## Возможности
✅ **Скрытие Menu Bar** (File, Edit, Assets, Tools...) через Windows API  
✅ **Скрытие Title Bar** (заголовок окна) через Windows API  
✅ **Настройки** для автоматического скрытия при запуске  
✅ **Простое управление** через меню Tools  

## Использование

### Через меню:
- `Tools → Editor UI → Hide Menu Bar` - скрыть главное меню
- `Tools → Editor UI → Show Menu Bar` - показать главное меню  
- `Tools → Editor UI → Hide Title Bar` - скрыть заголовок окна
- `Tools → Editor UI → Show Title Bar` - показать заголовок окна
- `Tools → Editor UI → Hide Both` - скрыть и меню и заголовок  
- `Tools → Editor UI → Show Both` - показать все обратно
- `Tools → Editor UI → Settings` - настройки  

## Настройки

В настройках (`EditorUISettings`) можно включить/выключить:
- Автоматическое скрытие Menu Bar при запуске Unity
- Автоматическое скрытие Title Bar при запуске Unity  
- Debug логирование

## Что работает:

1. **Menu Bar скрытие** ✅ - убирает строку с File, Edit, Assets, Tools...
2. **Title Bar скрытие** ✅ - убирает заголовок окна с названием проекта  
3. **Автозапуск** ✅ - настройка автоматического скрытия при запуске  

## Примечания:
- Работает только на Windows (использует Windows API)
- После скрытия Menu Bar может потребоваться перезапуск Unity для полного восстановления
- Title Bar скрывается навсегда до восстановления или перезапуска Unity  

## Быстрый старт:
1. Перейдите в `Tools → Editor UI → Hide Menu Bar`  
2. Или `Tools → Editor UI → Hide Both` чтобы убрать и меню и заголовок  
3. Настройки в `Tools → Editor UI → Settings`

## Восстановление:
- `Tools → Editor UI → Show Both` - восстановить все
- Или перезапустить Unity