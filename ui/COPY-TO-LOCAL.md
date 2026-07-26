# Copy this UI to your local machine

Target folder:

```text
D:\Delino\Sources\Vendo-FormBuilder-Ui
```

```powershell
New-Item -ItemType Directory -Force -Path "D:\Delino\Sources\Vendo-FormBuilder-Ui" | Out-Null
Copy-Item -Path ".\ui\*" -Destination "D:\Delino\Sources\Vendo-FormBuilder-Ui" -Recurse -Force
cd D:\Delino\Sources\Vendo-FormBuilder-Ui
npm install
npm run dev
```

Run from your local FormBuilder clone root. Full notes: [`ui/README.md`](./ui/README.md).
