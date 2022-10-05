FROM node:16-alpine as build
EXPOSE 7003

RUN mkdir -p /app
WORKDIR /app
COPY ./src/Apps/Logistics.OfficeApp/package*.json /app

RUN npm install --force

COPY ./src/Apps/Logistics.OfficeApp /app
CMD npm run docker:dev

#FROM nginx:1.23-alpine
#COPY --from=build /app/dist/logistics.office-app /usr/share/nginx/html
