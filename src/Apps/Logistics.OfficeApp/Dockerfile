FROM node:16-alpine as build

RUN mkdir -p /app
WORKDIR /app
COPY ./src/Apps/Logistics.OfficeApp/package*.json /app

RUN npm install --force
COPY ./src/Apps/Logistics.OfficeApp /app

RUN npm run build
 
FROM nginx:1.23-alpine
COPY --from=build /app/dist/logistics.office-app /usr/share/nginx/html
