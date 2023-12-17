import matplotlib.pyplot as plt
import numpy as np

phi_values = np.linspace(0, np.pi / 4, 3)
theta_values = np.linspace(0, np.pi / 2, 3)

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

x_values = []
y_values = []
z_values = []

point_number = 3
for phi in phi_values[1:]:
    for theta in theta_values:
        # Convert spherical to cartesian coordinates
        x = 3.0 * np.sin(phi) * np.cos(theta)
        y = 3.0 * np.sin(phi) * np.sin(theta)
        z = 3.0 * np.cos(phi)

        # x = theta
        # y = phi
        # z = 0.0

        # Append to lists
        x_values.append(x)
        y_values.append(y)
        z_values.append(z)

        # Add point number
        ax.text(x, y, z, str(point_number), fontdict={'color': 'blue'})
        point_number += 1


for phi in phi_values[1:]:
    for theta in theta_values:
        # Convert spherical to cartesian coordinates
        x = 2.0 * np.sin(phi) * np.cos(theta)
        y = 2.0 * np.sin(phi) * np.sin(theta)
        z = 2.0 * np.cos(phi)

        # x = theta
        # y = phi
        # z = 0.0

        # Append to lists
        x_values.append(x)
        y_values.append(y)
        z_values.append(z)

        # Add point number
        ax.text(x, y, z, str(point_number), fontdict={'color': 'red'})
        point_number += 1


for phi in phi_values[1:]:
    for theta in theta_values:
        # Convert spherical to cartesian coordinates
        x = np.sin(phi) * np.cos(theta)
        y = np.sin(phi) * np.sin(theta)
        z = np.cos(phi)
        
        # x = theta
        # y = phi
        # z = 0.0

        # Append to lists
        x_values.append(x)
        y_values.append(y)
        z_values.append(z)

        # Add point number
        ax.text(x, y, z, str(point_number), fontdict={'color': 'green'})
        point_number += 1

# Plot
ax.scatter(x_values, y_values, z_values, marker='.')
ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')
plt.show()