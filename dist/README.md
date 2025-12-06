## Release Process (for maintainers)

1. Make changes and test thoroughly
2. Run the build script:
   ```bash
   ./build.sh
   ```
3. The script will build and copy the DLL to `dist/`
4. Commit and push:
   ```bash
   git add dist/challange_mode.dll
   git commit -m "Release: version description"
   git push origin master
   ```

Users can always download the latest build from the `dist/` folder.