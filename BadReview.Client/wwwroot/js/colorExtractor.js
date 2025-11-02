// Color extractor using canvas
window.colorExtractor = {
    extractColors: function (imageUrl) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.crossOrigin = "Anonymous";
            
            img.onload = function() {
                try {
                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    
                    canvas.width = img.width;
                    canvas.height = img.height;
                    ctx.drawImage(img, 0, 0);
                    
                    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    const pixels = imageData.data;
                    
                    // Extract dominant colors
                    const colorMap = {};
                    
                    // Sample pixels (every 10th pixel for performance)
                    for (let i = 0; i < pixels.length; i += 40) {
                        const r = pixels[i];
                        const g = pixels[i + 1];
                        const b = pixels[i + 2];
                        const a = pixels[i + 3];
                        
                        // Skip transparent and very dark/bright pixels
                        if (a < 128 || (r < 30 && g < 30 && b < 30) || (r > 225 && g > 225 && b > 225)) {
                            continue;
                        }
                        
                        const key = `${Math.floor(r / 10)},${Math.floor(g / 10)},${Math.floor(b / 10)}`;
                        colorMap[key] = (colorMap[key] || 0) + 1;
                    }
                    
                    // Get most common colors
                    const sortedColors = Object.entries(colorMap)
                        .sort((a, b) => b[1] - a[1])
                        .slice(0, 5);
                    
                    if (sortedColors.length === 0) {
                        resolve({ primary: '#667eea', secondary: '#764ba2' });
                        return;
                    }
                    
                    // Convert back to RGB
                    const dominantColor = sortedColors[0][0].split(',').map(n => parseInt(n) * 10);
                    const secondaryColor = sortedColors.length > 1 
                        ? sortedColors[1][0].split(',').map(n => parseInt(n) * 10)
                        : dominantColor;
                    
                    // Increase saturation
                    const primary = `rgb(${dominantColor[0]}, ${dominantColor[1]}, ${dominantColor[2]})`;
                    const secondary = `rgb(${secondaryColor[0]}, ${secondaryColor[1]}, ${secondaryColor[2]})`;
                    
                    resolve({ primary, secondary });
                } catch (error) {
                    console.error('Error extracting colors:', error);
                    resolve({ primary: '#667eea', secondary: '#764ba2' });
                }
            };
            
            img.onerror = function() {
                console.error('Error loading image');
                resolve({ primary: '#667eea', secondary: '#764ba2' });
            };
            
            img.src = imageUrl;
        });
    }
};
