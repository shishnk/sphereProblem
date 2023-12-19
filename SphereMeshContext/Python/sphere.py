import matplotlib.pyplot as plt

with open('points', 'r') as f:
    lines = f.readlines()

points = [line.strip().split() for line in lines]
x_values = [float(point[0]) for point in points]
y_values = [float(point[1]) for point in points]
z_values = [float(point[2]) for point in points]

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

ax.scatter(x_values, y_values, z_values)

for i in range(len(points)):
    ax.text(x_values[i], y_values[i], z_values[i], str(i))

plt.show()