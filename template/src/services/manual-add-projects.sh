#!/usr/bin/env bash
set -e

# ======== Цвета ========
GREEN="\033[0;32m"
YELLOW="\033[0;33m"
BLUE="\033[0;34m"
RESET="\033[0m"

# ======== Папки ========
SERVICES_DIR="$(dirname "$0")"
ALL_SOLUTION=$(find "$SERVICES_DIR" -name "*.All.sln" | head -n 1)

if [ -z "$ALL_SOLUTION" ]; then
    echo -e "${YELLOW}Файл All.sln не найден. Создайте его вручную.${RESET}"
    exit 1
fi

echo -e "${BLUE}Используется решение:${RESET} $ALL_SOLUTION"
echo -e "${BLUE}Проверка проектов сервисов...${RESET}"

# Получаем список уже добавленных проектов (нормализуем слэши)
existing=$(dotnet sln "$ALL_SOLUTION" list | sed 's|\\|/|g')

# Проходим по всем папкам сервисов
for service_dir in "$SERVICES_DIR"/*; do
    [ -d "$service_dir" ] || continue

    # Вычисляем короткое имя сервиса (после последней точки)
    full_folder=$(basename "$service_dir")              # NamespaceRoot.ProductName.ServiceNameOrCustom
    folder_name="${full_folder##*.}"                   # ServiceNameOrCustom

    # Ищем все csproj внутри папки сервиса (макс. 2 уровня)
    for proj in $(find "$service_dir" -maxdepth 2 -name "*.csproj"); do
        proj_rel=$(realpath --relative-to="$(dirname "$ALL_SOLUTION")" "$proj" | sed 's|\\|/|g')
        proj_name=$(basename "$proj")

        if ! echo "$existing" | grep -qF "$proj_rel"; then
            dotnet sln "$ALL_SOLUTION" add "$proj" --solution-folder "$folder_name" >/dev/null
            echo -e "${GREEN}Добавлено:${RESET} $proj_name → $folder_name"
        else
            echo -e "${YELLOW}Уже подключено:${RESET} $proj_name"
        fi
    done
done

echo -e "${BLUE}Все проекты проверены.${RESET}"
read -n1 -r -p "Нажмите любую клавишу для выхода..."
echo
