# Stage 1: Build the Angular application
FROM node:latest as build-stage

# Set the working directory
WORKDIR /app

# Copy package.json and package-lock.json to install dependencies
COPY package.json package-lock.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the Angular application
RUN npm run build -- --configuration=production

# Stage 2: Serve the application using NGINX
FROM nginx:latest as production-stage

# Set the working directory inside the container
WORKDIR /usr/share/nginx/html

# Copy the Angular build output to NGINX's default directory
COPY --from=build-stage /app/dist/jackpot-plot-web/browser .

# Replace the default NGINX configuration with a custom configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for the web server
EXPOSE 80

# Start NGINX
CMD ["nginx", "-g", "daemon off;"]
