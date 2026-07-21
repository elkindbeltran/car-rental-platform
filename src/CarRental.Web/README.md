# Drively — Car Rental SPA

Angular 20 standalone SPA for the Car Rental API. It includes Auth0 PKCE authentication, a responsive fleet
catalog, vehicle and customer administration, and an end-to-end booking flow.

## Local development

1. Configure the Auth0 application with `http://localhost:4200` as an allowed callback, logout, and web origin.
2. Start `CarRental.API` with its HTTPS profile on `https://localhost:7071`.
3. Run `npm install` and `npm start` in this directory.

Runtime endpoints and public Auth0 SPA identifiers are kept in `src/environments/environment.ts`; never place
client secrets in an Angular application. API calls use the Auth0 interceptor allow-list, so tokens are not sent
to unrelated hosts.

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.1.6.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
