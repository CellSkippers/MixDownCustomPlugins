# MixedDownCustomPlugins

## Contributing

Change the target r2modman profile in
`MixedDownCustomPlugins/MixedDownCustomPlugins.csproj` to your desired
development profile.

In order to prevent git from trying to include this change you can run:
```sh
git update-index --assume-unchanged MixedDownCustomPlugins/MixedDownCustomPlugins.csproj
```

Note that this will mean that any actual legitimate changes to this file won't
be noticed, you'll have to add those manually.
