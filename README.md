c# .NET 6 MVVM 

Приложение для поиска дубликатов музыкальных файлов. 

- Prism для MVVM
- SoundFingerprinting для создания отпечатков
- Soundfingerprinting.LMDB для хранения значений отпечатков в LMDB
- Spectrogram для создания спектрограмм файлов
- TagLib# для чтения и изменения метаданных файлов

Основной код содержится в папке HarmonyApp

Запуск через командную строку с параметрами: 
![console](https://github.com/arsenyrogatov/HarmonyApp/assets/89843046/3a7a4342-cab4-4d57-b219-6e70c80e1d74)

Запуск в режиме графического интерфейса: 
![main](https://github.com/arsenyrogatov/HarmonyApp/assets/89843046/55b41b0b-f643-4bee-9300-d0f0f023e8cb)
Выбор директорий для поиска дубликатов

![scanresult](https://github.com/arsenyrogatov/HarmonyApp/assets/89843046/4a83e7e2-f57c-4e7c-b8f5-577daf72fc4f)
Найденные дубликаты

![spectrogram](https://github.com/arsenyrogatov/HarmonyApp/assets/89843046/f8853c55-da8b-4b19-b94e-5acd4f8f8a08)
Просмотр спектрограммы для музыкального файла
