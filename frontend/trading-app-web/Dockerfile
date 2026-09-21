# ============ Stage 1: Build ============
FROM node:22-alpine AS build

WORKDIR /app

# Package files
COPY package.json package-lock.json* ./

# Install deps
RUN npm ci

# Copy source
COPY . .

# Build
RUN npm run build

# ============ Stage 2: Serve with nginx ============
FROM nginx:alpine

# nginx conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Built files
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]